using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Wixpack.Core.Configuration;
using Wixpack.Telegram.Commands;
using Wixpack.Telegram.Floket;
using Wixpack.Telegram.Handlers;
using Wixpack.Telegram.Services;

namespace Wixpack.Telegram.DependencyInjection;

public static class TelegramServiceCollectionExtensions
{
    public static IServiceCollection AddWixpackTelegram(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = new WixpackSettings();
        configuration.Bind(settings);

        var envToken = Environment.GetEnvironmentVariable("WIXPACK_Telegram__BotToken");
        var token = !string.IsNullOrWhiteSpace(envToken)
            ? envToken.Trim()
            : "0000000000:PLACEHOLDER_TOKEN_REPLACE_IN_ENV";

        services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(token));

        services.AddSingleton<ICommandHandler, StartCommand>();
        services.AddSingleton<ICommandHandler, HelpCommand>();
        services.AddSingleton<ICommandHandler, GameCommand>();
        services.AddSingleton<ICommandHandler, DlCommand>();

        services.AddSingleton<FloketGroupHandler>();
        services.AddSingleton<CallbackQueryHandler>();
        services.AddHostedService<TelegramBotService>();

        return services;
    }
}
