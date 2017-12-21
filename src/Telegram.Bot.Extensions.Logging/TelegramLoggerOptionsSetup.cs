using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Telegram.Bot.Extensions.Logging
{
    public class TelegramLoggerOptionsSetup : ConfigureFromConfigurationOptions<TelegramLoggerOptions>
    {
        public TelegramLoggerOptionsSetup(IConfiguration config) : base(config)
        {
        }
    }
}
