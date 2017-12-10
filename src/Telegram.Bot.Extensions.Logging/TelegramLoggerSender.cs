using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegram.Bot.Extensions.Logging
{
    public class TelegramLoggerSender : IDisposable
    {
        private const int MaxQueuedMessages = 1024;

        private readonly BlockingCollection<string> _messageQueue = new BlockingCollection<string>(MaxQueuedMessages);

        private readonly ITelegramBotClient _botClient;
        private readonly ChatId _chatId;

        private readonly Task _outputTask;

        public TelegramLoggerSender(ITelegramBotClient botClient, ChatId chatId)
        {
            // Start Telegram message queue processor
            _botClient = botClient;
            _chatId = chatId;

            _outputTask = Task.Factory.StartNew(
                ProcessLogQueue,
                this,
                TaskCreationOptions.LongRunning);
        }

        public void EnqueueMessage(string message)
        {
            if (!_messageQueue.IsAddingCompleted)
            {
                try
                {
                    _messageQueue.Add(message);
                    return;
                }
                catch (InvalidOperationException) { }
            }

            // Adding is completed so just log the message
            WriteMessage(message);
        }

        private void WriteMessage(string message)
        {
            try
            {
                Task.Run(() => _botClient.SendTextMessageAsync(_chatId, message));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void ProcessLogQueue()
        {
            foreach (var message in _messageQueue.GetConsumingEnumerable())
            {
                WriteMessage(message);
            }
        }

        private static void ProcessLogQueue(object state)
        {
            var telegramLogger = (TelegramLoggerSender)state;

            telegramLogger.ProcessLogQueue();
        }

        public void Dispose()
        {
            _messageQueue.CompleteAdding();

            try
            {
                _outputTask.Wait(1500);
            }
            catch (TaskCanceledException) { }
            catch (AggregateException ex)
                when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException) { }
        }
    }
}
