using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Telegram.Bot.Extensions.Logging
{
    public class TelegramLogger : ILogger
    {
        private readonly string _category;
        private readonly Func<string, LogLevel, bool> _filter;
        private readonly TelegramLoggerSender _messageQueue;
        private readonly TelegramLoggerOptions _options;
        private StringBuilder _logBuilder;

        internal TelegramLogger(
            string category,
            TelegramLoggerSender messageQueue,
            TelegramLoggerOptions options,
            Func<string, LogLevel, bool> filter)
        {
            _category = category ?? throw new ArgumentNullException(nameof(category));
            _messageQueue = messageQueue;
            _filter = filter ?? ((cat, logLevel) => true);
            _options = options;
        }

        public TelegramLogger(
            string category,
            ITelegramBotClient botClient,
            TelegramLoggerOptions options,
            Func<string, LogLevel, bool> filter)
            : this(category, new TelegramLoggerSender(botClient, options.ChatId), options, filter)
        { }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            if (!string.IsNullOrEmpty(message) && !string.IsNullOrWhiteSpace(message))
            {
                SendMessage(logLevel, _category, eventId.Id, message, exception);
            }
        }

        private void SendMessage(LogLevel logLevel, string logName, int eventId, string message, Exception exception)
        {
            var logBuilder = _logBuilder;
            _logBuilder = null;

            if (logBuilder == null)
            {
                logBuilder = new StringBuilder();
            }

            var logLevelString = GetLogLevelString(logLevel);

            logBuilder.AppendLine($"Log source: {_options.SourceName}");

            if (!string.IsNullOrEmpty(logLevelString))
            {
                logBuilder.Append($"{logLevelString}: ");
            }

            logBuilder.Append(logName);
            logBuilder.Append("[");
            logBuilder.Append(eventId);
            logBuilder.AppendLine("]");

            if (!string.IsNullOrEmpty(message))
            {
                logBuilder.AppendLine($"\n{message}");
            }

            if (exception != null)
            {
                logBuilder.AppendLine();
                logBuilder.AppendLine(exception.ToString());
            }

            if (logBuilder.Length == 0)
            {
                return;
            }

            if (logBuilder.Length > 4096)
            {
                logBuilder.Remove(4080, logBuilder.Length);
                logBuilder.Append("...\n...");
            }

            var content = logBuilder.ToString();

            _messageQueue.EnqueueMessage(content);

            logBuilder.Clear();
            if (logBuilder.Capacity > 1024)
            {
                logBuilder.Capacity = 1024;
            }
            _logBuilder = logBuilder;
        }


        private static string GetLogLevelString(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return "trce";
                case LogLevel.Debug:
                    return "dbug";
                case LogLevel.Information:
                    return "info";
                case LogLevel.Warning:
                    return "warn";
                case LogLevel.Error:
                    return "fail";
                case LogLevel.Critical:
                    return "crit";
                default:
                    return null;
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _filter(_category, logLevel);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}
