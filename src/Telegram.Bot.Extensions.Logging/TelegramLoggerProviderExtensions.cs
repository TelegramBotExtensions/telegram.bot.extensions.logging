using System;
using Microsoft.Extensions.Logging;

namespace Telegram.Bot.Extensions.Logging
{
    public static class TelegramLoggerProviderExtensions
    {
        public static ILoggerFactory AddTelegramLogger(
            this ILoggerFactory loggerFactory,
            TelegramLoggerOptions options,
            Func<string, LogLevel, bool> filter = default)
        {
            var botClient = new TelegramBotClient(options.BotToken);
            loggerFactory?.AddProvider(new TelegramLoggerProvider(botClient, options, filter));
            return loggerFactory;
        }

        public static ILoggerFactory AddTelegramLogger(
            this ILoggerFactory loggerFactory,
            Action<TelegramLoggerOptions> configure,
            Func<string, LogLevel, bool> filter = default)
        {
            var options = new TelegramLoggerOptions();
            configure(options);
            return loggerFactory?.AddTelegramLogger(options, filter);
        }
    }
}
