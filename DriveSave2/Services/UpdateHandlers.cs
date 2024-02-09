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
        private static readonly string yandexWeatherApiKey = "KEY";
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
     
                { Message: { } message } when message.Text.StartsWith("/weather") => SendWeatherData(message),
                { Message: { } message } when message.Text.StartsWith("/погода") => TemperatureInCity(message),
                { Message: { } message } => EchoMessage(message),
                _ => Task.CompletedTask
            };

            await handler;
        }

        private async Task SendWeatherData(Message message, CancellationToken cancellationToken)
        {
            string weatherData = await GetWeatherAsync(location);
            Console.WriteLine("------------------------------------");
            Console.WriteLine("--------------------", weatherData);

            const int maxLength = 4000;  // Например, ограничиваем до 4000 символов
            if (weatherData.Length > maxLength)
            {
                weatherData = weatherData.Substring(0, maxLength);
                Console.WriteLine("--0----0-----0--------0----0------");
                Console.WriteLine("---0-----0----0----", weatherData);
            }

            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: weatherData,
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

        private static async Task<string> GetWeatherAsync(string location)
        {
            string apiUrl = $"https://api.weather.yandex.ru/v2/forecast/?lat=55.721560&lon=84.929553&lang=ru_RU\",false,$context";

            using (var client = new WebClient())
            {
                client.Headers.Add("X-Yandex-API-Key", yandexWeatherApiKey);
                var response = await client.DownloadStringTaskAsync(apiUrl);
                return response;
            }
        }


        private static string TemperatureInCity(Message message, CancellationToken cancellationToken)
        {
            string result;
            string cityName = "Краснодар";
            switch (cityName.ToLower())
            {
                case "/столбцы": result = GetTemperature(url: @"https://xml.meteoservice.ru/export/gismeteo/point/9189.xml"); break;
                case "/минск": result = GetTemperature(url: @"https://xml.meteoservice.ru/export/gismeteo/point/34.xml"); break;
                case "/барановичи": result = GetTemperature(url: @"https://xml.meteoservice.ru/export/gismeteo/point/9156.xml"); break;
                default: result = "Такого города я не знаю\n/столбцы\n/минск\n/барановичи\n"; break;
            }

            return result;
        }


        private static string GetTemperature(string url)
        {
            string xml = new WebClient().DownloadString(url);
            /*Console.WriteLine(xml);*/

            var doc = XDocument.Parse(xml);
            var forecast = doc.Descendants("MMWEATHER")
                               .Descendants("REPORT")
                               .Descendants("TOWN")
                               .Descendants("FORECAST")
                               .ToList()[0];

            //Console.WriteLine(town);

            string temperatureMax = forecast.Element("TEMPERATURE").Attribute("max").Value;
            string temperatureMin = forecast.Element("TEMPERATURE").Attribute("min").Value;

            string result = $"Температура в городе от {temperatureMin} до {temperatureMax}";
            return result;
        }
    }
}