using Telegram.Bot.Types;

namespace Telegram.Bot.Extensions.Logging
{
    public class TelegramLoggerOptions
    {
        public string BotToken { get; set; }

        /// <summary>
        /// Unique identifier for the target chat or username of the target channel (in the format @channelusername)
        /// </summary>
        public ChatId ChatId { get; set; }

        /// <summary>
        /// The name of the source of logs
        /// </summary>
        public string SourceName { get; set; }
    }
}
