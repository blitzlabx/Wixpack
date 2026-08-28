using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Wixpack.Floket.Models;
using Wixpack.Floket.Services;
using Wixpack.Floket.Telegram;
using Wixpack.Telegram.Keyboards;

namespace Wixpack.Telegram.Floket;

/// <summary>
/// Handles group member joins and Floket verification callbacks.
/// </summary>
public sealed class FloketGroupHandler
{
    private readonly FloketVerificationService _floket;
    private readonly ILogger<FloketGroupHandler> _logger;

    public FloketGroupHandler(FloketVerificationService floket, ILogger<FloketGroupHandler> logger)
    {
        _floket = floket;
        _logger = logger;
    }

    public async Task OnChatMemberUpdatedAsync(ITelegramBotClient bot, ChatMemberUpdated update, CancellationToken ct)
    {
        // New member joined (or was added)
        var oldStatus = update.OldChatMember.Status;
        var newStatus = update.NewChatMember.Status;

        var becameMember =
            newStatus is ChatMemberStatus.Member or ChatMemberStatus.Restricted
            && oldStatus is ChatMemberStatus.Left or ChatMemberStatus.Kicked;

        if (!becameMember)
            return;

        var user = update.NewChatMember.User;
        if (user.IsBot)
            return;

        var chatId = update.Chat.Id;
        var config = await _floket.ResolveConfigAsync(chatId, ct);
        if (!config.Enabled)
            return;

        if (await _floket.IsVerifiedAsync(user.Id, chatId, ct))
            return;

        // Restrict until verified
        if (config.RestrictUntilVerified)
        {
            try
            {
                await bot.RestrictChatMember(
                    chatId,
                    user.Id,
                    new ChatPermissions
                    {
                        CanSendMessages = false,
                        CanSendAudios = false,
                        CanSendDocuments = false,
                        CanSendPhotos = false,
                        CanSendVideos = false,
                        CanSendVideoNotes = false,
                        CanSendVoiceNotes = false,
                        CanSendPolls = false,
                        CanSendOtherMessages = false,
                        CanAddWebPagePreviews = false,
                        CanChangeInfo = false,
                        CanInviteUsers = false,
                        CanPinMessages = false,
                        CanManageTopics = false
                    },
                    cancellationToken: ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not restrict user {UserId} in {ChatId}. Is bot admin with restrict rights?", user.Id, chatId);
            }
        }

        var start = await _floket.StartVerificationAsync(user.Id, chatId, user.Username, user.FirstName, ct);
        if (!start.Success || start.Value is null)
        {
            _logger.LogDebug("Floket start skipped: {Error}", start.Error);
            return;
        }

        var session = start.Value;
        var markup = BuildChallengeKeyboard(session);

        try
        {
            await bot.SendMessage(
                chatId,
                FloketTelegramMessages.ChallengeMessage(session),
                parseMode: ParseMode.Html,
                replyMarkup: markup,
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send Floket challenge in {ChatId}", chatId);
        }
    }

    public async Task OnCallbackAsync(ITelegramBotClient bot, CallbackQuery query, CancellationToken ct)
    {
        if (query.Data is null || query.From is null || query.Message is null)
            return;

        if (!query.Data.StartsWith("floket:", StringComparison.Ordinal))
            return;

        // floket:ans:{sessionId}:{answer}
        // floket:expire_check:{sessionId}
        var parts = query.Data.Split(':', 4);
        if (parts.Length < 3)
        {
            await bot.AnswerCallbackQuery(query.Id, cancellationToken: ct);
            return;
        }

        var action = parts[1];
        var sessionId = parts[2];

        if (action == "ans" && parts.Length >= 4)
        {
            var answer = parts[3];
            var result = await _floket.SubmitAnswerAsync(sessionId, answer, ct);

            if (result.Success)
            {
                await UnrestrictAsync(bot, query.Message.Chat.Id, query.From.Id, ct);
                try
                {
                    await bot.EditMessageText(
                        query.Message.Chat.Id,
                        query.Message.MessageId,
                        FloketTelegramMessages.SuccessMessage(),
                        parseMode: ParseMode.Html,
                        cancellationToken: ct);
                }
                catch { /* message may be old */ }

                await bot.AnswerCallbackQuery(query.Id, "Verified by Floket ✓", cancellationToken: ct);
            }
            else
            {
                var failedHard = result.Error?.Contains("Too many", StringComparison.OrdinalIgnoreCase) == true
                                 || result.Error?.Contains("expired", StringComparison.OrdinalIgnoreCase) == true;

                if (failedHard)
                {
                    try
                    {
                        await bot.EditMessageText(
                            query.Message.Chat.Id,
                            query.Message.MessageId,
                            FloketTelegramMessages.FailureMessage(result.Error ?? "Denied"),
                            parseMode: ParseMode.Html,
                            cancellationToken: ct);
                    }
                    catch { }

                    var config = await _floket.ResolveConfigAsync(query.Message.Chat.Id, ct);
                    if (config.KickOnFailure)
                    {
                        try
                        {
                            await bot.BanChatMember(query.Message.Chat.Id, query.From.Id, cancellationToken: ct);
                            await bot.UnbanChatMember(query.Message.Chat.Id, query.From.Id, onlyIfBanned: true, cancellationToken: ct);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Kick after Floket failure failed for {UserId}", query.From.Id);
                        }
                    }
                }

                await bot.AnswerCallbackQuery(query.Id, result.Error ?? "Wrong", showAlert: true, cancellationToken: ct);
            }
            return;
        }

        await bot.AnswerCallbackQuery(query.Id, cancellationToken: ct);
    }

    private static InlineKeyboardMarkup BuildChallengeKeyboard(VerificationSession session)
    {
        // Build 3 options: correct + 2 distractors as buttons (green not revealed)
        if (!int.TryParse(session.CorrectAnswer, out var correct))
        {
            // Fallback: single text-style not possible; use numeric near answers
            correct = 0;
        }

        var options = new HashSet<string> { session.CorrectAnswer };
        var rng = Random.Shared;
        while (options.Count < 3)
        {
            var delta = rng.Next(1, 6) * (rng.Next(0, 2) == 0 ? 1 : -1);
            var wrong = (correct + delta).ToString();
            if (wrong != session.CorrectAnswer && int.Parse(wrong) > 0)
                options.Add(wrong);
        }

        var shuffled = options.OrderBy(_ => rng.Next()).ToList();
        var row = shuffled.Select(a =>
            ButtonStyles.Callback(a, $"floket:ans:{session.SessionId}:{a}")).ToArray();

        return new InlineKeyboardMarkup([
            row,
            [
                ButtonStyles.Callback("« Powered by Floket", $"floket:info:{session.SessionId}")
            ]
        ]);
    }

    private async Task UnrestrictAsync(ITelegramBotClient bot, long chatId, long userId, CancellationToken ct)
    {
        try
        {
            await bot.RestrictChatMember(
                chatId,
                userId,
                new ChatPermissions
                {
                    CanSendMessages = true,
                    CanSendAudios = true,
                    CanSendDocuments = true,
                    CanSendPhotos = true,
                    CanSendVideos = true,
                    CanSendVideoNotes = true,
                    CanSendVoiceNotes = true,
                    CanSendPolls = true,
                    CanSendOtherMessages = true,
                    CanAddWebPagePreviews = true,
                    CanInviteUsers = true
                },
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unrestrict failed for {UserId} in {ChatId}", userId, chatId);
        }
    }
}
