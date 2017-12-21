using System;
using System.Net.NetworkInformation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

        public static ILoggingBuilder AddTelegramLogger(this ILoggingBuilder builder)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TelegramLoggerProvider>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<TelegramLoggerOptions>, TelegramLoggerOptionsSetup>());
//            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsChangeTokenSource<ConsoleLoggerOptions>, LoggerProviderOptionsChangeTokenSource<ConsoleLoggerOptions, ConsoleLoggerProvider>>());
        }
    }
}
