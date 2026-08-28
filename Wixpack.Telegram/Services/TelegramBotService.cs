using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Wixpack.Core.Configuration;
using Wixpack.Telegram.Commands;
using Wixpack.Telegram.Floket;
using Wixpack.Telegram.Handlers;

namespace Wixpack.Telegram.Services;

public sealed class TelegramBotService : BackgroundService
{
    private readonly ITelegramBotClient _bot;
    private readonly IEnumerable<ICommandHandler> _commands;
    private readonly CallbackQueryHandler _callbacks;
    private readonly FloketGroupHandler _floketGroups;
    private readonly WixpackSettings _settings;
    private readonly ILogger<TelegramBotService> _logger;

    public TelegramBotService(
        ITelegramBotClient bot,
        IEnumerable<ICommandHandler> commands,
        CallbackQueryHandler callbacks,
        FloketGroupHandler floketGroups,
        IOptions<WixpackSettings> settings,
        ILogger<TelegramBotService> logger)
    {
        _bot = bot;
        _commands = commands;
        _callbacks = callbacks;
        _floketGroups = floketGroups;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.Telegram.BotToken)
            || _settings.Telegram.BotToken.Contains("PLACEHOLDER", StringComparison.Ordinal))
        {
            _logger.LogWarning("Telegram BotToken is empty. Set it in settings.json or WIXPACK_Telegram__BotToken. Bot will not start polling.");
            return;
        }

        if (!_settings.Telegram.EnablePolling)
        {
            _logger.LogInformation("Telegram polling disabled in settings.");
            return;
        }

        var me = await _bot.GetMe(stoppingToken);
        _logger.LogInformation("Telegram bot online as @{Username} (id {Id})", me.Username, me.Id);

        var botCommands = _commands
            .Select(c => new BotCommand { Command = c.Command, Description = c.Description })
            .ToArray();
        await _bot.SetMyCommands(botCommands, cancellationToken: stoppingToken);

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates =
            [
                UpdateType.Message,
                UpdateType.CallbackQuery,
                UpdateType.ChatMember,
                UpdateType.MyChatMember
            ],
            DropPendingUpdates = true
        };

        _bot.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandleErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken);

        _logger.LogInformation("Telegram long-polling started (incl. Floket group joins).");

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Telegram bot service stopping.");
        }
    }

    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        try
        {
            if (update.CallbackQuery is { } cq)
            {
                await _callbacks.HandleAsync(bot, cq, ct);
                return;
            }

            if (update.ChatMember is { } memberUpdate)
            {
                await _floketGroups.OnChatMemberUpdatedAsync(bot, memberUpdate, ct);
                return;
            }

            if (update.Message is { Text: { } text } message)
            {
                if (text.StartsWith('/'))
                {
                    await HandleCommandAsync(bot, message, text, ct);
                    return;
                }

                // Private chat: bare URL → treat as /dl
                if (message.Chat.Type == ChatType.Private
                    && Uri.TryCreate(text.Trim(), UriKind.Absolute, out var uri)
                    && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                {
                    var dl = _commands.FirstOrDefault(c => c.Command == "dl");
                    if (dl is not null)
                        await dl.HandleAsync(bot, message, text.Trim(), ct);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error processing update {UpdateId}", update.Id);
        }
    }

    private async Task HandleCommandAsync(ITelegramBotClient bot, Message message, string text, CancellationToken ct)
    {
        var parts = text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var cmd = parts[0].TrimStart('/').Split('@')[0].ToLowerInvariant();
        var args = parts.Length > 1 ? parts[1] : null;

        var handler = _commands.FirstOrDefault(c =>
            string.Equals(c.Command, cmd, StringComparison.OrdinalIgnoreCase));

        if (handler is null)
            return;

        if (handler.AdminOnly && !IsAdmin(message.From?.Id))
        {
            await bot.SendMessage(message.Chat.Id, "This command is admin-only.", cancellationToken: ct);
            return;
        }

        await handler.HandleAsync(bot, message, args, ct);
    }

    private bool IsAdmin(long? userId)
    {
        if (userId is null) return false;
        return _settings.Telegram.AdminUserIds.Contains(userId.Value.ToString());
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken ct)
    {
        _logger.LogError(exception, "Telegram polling error");
        return Task.CompletedTask;
    }
}
