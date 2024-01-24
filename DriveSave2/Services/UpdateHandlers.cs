using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace DriveSave2.Services
{
    public class UpdateHandlers
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<UpdateHandlers> _logger;
        private static readonly string yandexWeatherApiKey = "apiKey";

        // метод инициализации двух полей. 
        public UpdateHandlers(ITelegramBotClient botClient, ILogger<UpdateHandlers> logger)
        {
            _botClient = botClient; //_botClient - для взаимодействия с API Telegram бота
            _logger = logger;  // _logger - для логирования
        }

        // метод обработки ошибок
        public Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)

        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);
            return Task.CompletedTask;
        }

        // метод обрабатывает обновления от Telegram бота
        public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
        {
            var handler = update switch
            {
                // если обновление содержит сообщение (Message),
                // то вызывается метод EchoMessage для его обработки.
                { Message: { } message } => EchoMessage(message, cancellationToken),
               
            };

            await handler;
        }

        // метод эхо-ответа
        private async Task EchoMessage(Message message, CancellationToken cancellationToken)
        {

            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: message.Text, 
                cancellationToken: cancellationToken);
        }







    }


}
