using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Project_Pineapplesummer.Modules.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Project_Pineapplesummer.Modules.Command_s
{
    public class WeatherCommand : ModuleBase<SocketCommandContext>
    {
        ErrorServices errorServices = new ErrorServices();
        string apikey = ".";

        [Command("Weather")]
        public async Task GetWeather([Remainder] string city)
        {
            HttpClient httpClient = new HttpClient();
            Root obj;

            string icon = ":sunny:";

            string uri = $"weather?q={city}&appid={apikey}";

            httpClient.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/");

            try
            {
                string json = await httpClient.GetStringAsync(uri);
                Console.WriteLine(json);
                obj = JsonConvert.DeserializeObject<Root>(json);
            }
            catch(Exception ex)
            {
                await errorServices.SendErrorMessage(ex.Message + $"\n\n URI: {uri.Replace(apikey, "Redacted")}", "PPSc0xWcomRg", Context.Channel, ErrorServices.severity.Error);
                return;
            }

            switch(obj.weather[0].icon)
            {
                case "01d":
                    icon = ":sunny:";
                    break;
                case "02d":
                    icon = ":partly_sunny:";
                    break;
                case "03d":
                    icon = ":white_sun_cloud:";
                    break;
                case "04d":
                    icon = ":cloud:";
                    break;
                case "09d":
                    icon = ":cloud_rain:";
                    break;
                case "10d":
                    icon = ":white_sun_rain_cloud:";
                    break;
                case "11d":
                    icon = ":thunder_cloud_rain:";
                    break;
                case "13d":
                    icon = ":snowflake:";
                    break;
                case "50d":
                    icon = ":foggy:";
                    break;
            }

            DateTime sunrise = DateTime.UnixEpoch.AddSeconds(obj.sys.sunrise).AddSeconds(obj.timezone);
            DateTime sunset = DateTime.UnixEpoch.AddSeconds(obj.sys.sunset).AddSeconds(obj.timezone);

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(Color.Green)
                .WithTitle($"{obj.name} :flag_{obj.sys.country.ToLower()}:")
                .WithCurrentTimestamp()
                .WithThumbnailUrl("https://openweathermap.org/themes/openweathermap/assets/img/logo_white_cropped.png")

                .AddField($"{icon} {obj.weather[0].main}", $"{Math.Round(obj.main.temp - 274.15)}°C", true)
                .AddField($":thermometer: Feelslike", $"{Math.Round(obj.main.feels_like - 274.15)}°C", true)
                .AddField($":sweat_drops: Humidity", $"{obj.main.humidity}%", true)

                .AddField($":dash: Wind", $"{obj.wind.speed} m/s", true)
                .AddField($":fog: Cloudiness", $"{obj.clouds.all}%", true)
                .AddField($":bar_chart: Pressure", $"{obj.main.pressure} hPa", true)

                .AddField($":sunrise: Sunrise", sunrise.ToShortTimeString(), true)
                .AddField($":night_with_stars: Sunset", sunset.ToShortTimeString(), true)
                .AddField($":map: Coordinates", $"lat:{obj.coord.lat} / lon:{obj.coord.lon}", true);

            await Context.Channel.SendMessageAsync(null, false, embed.Build());
        }

    }

    public class Coord
    {
        public double lon { get; set; }
        public double lat { get; set; }

    }

    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }

    }

    public class Main
    {
        public double temp { get; set; }
        public double feels_like { get; set; }
        public double temp_min { get; set; }
        public double temp_max { get; set; }
        public double pressure { get; set; }
        public double humidity { get; set; }

    }

    public class Wind
    {
        public double speed { get; set; }
        public double deg { get; set; }

    }

    public class Clouds
    {
        public int all { get; set; }

    }

    public class Sys
    {
        public int type { get; set; }
        public int id { get; set; }
        public double message { get; set; }
        public string country { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }

    }

    public class Root
    {
        public Coord coord { get; set; }
        public List<Weather> weather { get; set; }
        public string baze { get; set; }
        public Main main { get; set; }
        public Wind wind { get; set; }
        public Clouds clouds { get; set; }
        public int dt { get; set; }
        public Sys sys { get; set; }
        public int timezone { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int cod { get; set; }
    }
}

