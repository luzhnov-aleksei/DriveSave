using Newtonsoft.Json;
using System.Net;
using System.Threading;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DriveSave2.Services
{
    public class UpdateHandlers
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<UpdateHandlers> _logger;
        private static readonly string yandexWeatherApiKey = "key";
        private readonly string location = "Moscow";
        

        public UpdateHandlers(ITelegramBotClient botClient, ILogger<UpdateHandlers> logger)
        {
            _botClient = botClient;
            _logger = logger;
        }

        public Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogInformation("HandleError: {ErrorMessage}", errorMessage);
            return Task.CompletedTask;
        }

        public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
        {
            var handler = update switch
            {
     
                { Message: { } message } when message.Text.StartsWith("/weather") => SendWeatherData(message, cancellationToken),
                { Message: { } message } => EchoMessage(message, cancellationToken),
                _ => Task.CompletedTask
            };

            await handler;
        }
        //обращаемся к yandex api weather
        private async Task SendWeatherData(Message message, CancellationToken cancellationToken)
        {
            Weather weatherData = await GetWeatherAsync(location);
            string response = "Текущая температура в городе " + weatherData.GeoObject.Locality.Name + " " + weatherData.Fact.Temp +" градусов.";


            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: response,
                parseMode: ParseMode.Markdown,
                disableWebPagePreview: true
            );
        }

        private async Task EchoMessage(Message message, CancellationToken cancellationToken)
        {
     
            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Эхо ответ. Ты написал мне: \n "+ message.Text,
                cancellationToken: cancellationToken
            );
        }
        //достаем градусы по городу
        private static async Task<Weather> GetWeatherAsync(string location)
        {
            string apiUrl = $"https://api.weather.yandex.ru/v2/forecast/?lat=45.0401604&lon=38.9759647&lang=ru_RU";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Yandex-API-Key", yandexWeatherApiKey);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                HttpResponseMessage response = await client.SendAsync(request);
                Console.WriteLine(response.StatusCode);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string message = await response.Content.ReadAsStringAsync();
                    Weather weather = JsonConvert.DeserializeObject<Weather>(message);
                    return weather;
                }
                else
                {
                    return null;
                }
            }
        }

    }
}