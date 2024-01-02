using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpenWeatherC_
{
	public class Program
	{
		static async Task Main(string[] args)
		{
			try
			{
				var client = new HttpClient();

				var request = new HttpRequestMessage(HttpMethod.Get, "https://api.openweathermap.org/data/2.5/weather?lat=27.49&lon=-97.26&appid={api_key}");

				var response = await client.SendAsync(request);
				response.EnsureSuccessStatusCode();

				if (response.IsSuccessStatusCode)
				{
					var content = await response.Content.ReadAsStringAsync();
					var weatherData = JsonConvert.DeserializeObject<WeatherData>(content);

					var tempFar = Math.Round((weatherData.main.temp - 273.15) * 1.8 + 32);
					var wind = Math.Round((weatherData.wind.speed * 2.236936)); // convert to miles per hour
					var cloudVis = weatherData.clouds.all;
					var humidity = weatherData.main.humidity;
					var feelsLike = Math.Round((weatherData.main.feels_like - 273.15) * 1.8 + 32);
					var sunriseUtc = DateTimeOffset.FromUnixTimeSeconds(weatherData.sys.sunrise).UtcDateTime;
					var sunsetUtc = DateTimeOffset.FromUnixTimeSeconds(weatherData.sys.sunset).UtcDateTime;
				

					// Convert to Eastern Time
					TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
					var sunriseEastern = TimeZoneInfo.ConvertTimeFromUtc(sunriseUtc, easternZone);
					var sunsetEastern = TimeZoneInfo.ConvertTimeFromUtc(sunsetUtc, easternZone);
					int month = sunriseEastern.Month; // get month value

					// Handle weather data as needed
					Console.WriteLine(tempFar);
					Console.WriteLine(wind + " miles per hour");
					Console.WriteLine(cloudVis + "% visibility");
					Console.WriteLine(humidity + "% humidity");
					Console.WriteLine("feels like : " + feelsLike);
					Console.WriteLine("Sunrise: " + sunriseEastern.ToString("yyyy-MM-dd HH:mm:ss"));
					Console.WriteLine("Sunset: " + sunsetEastern.ToString("yyyy-MM-dd HH:mm:ss"));

					Console.Write("Ideal Fishing: ");
					bool fish = IsGoodFishing((float)wind, cloudVis, humidity, DateTime.Now, month);
					Console.WriteLine(fish);

					Console.Write("High winds: ");
					bool winds = HighWinds((float)wind);
					Console.WriteLine(winds);

				}
			}
			catch (HttpRequestException ex)
			{
				Console.WriteLine($"HTTP request failed: {ex.Message}");
			}
			catch (JsonException ex)
			{
				Console.WriteLine($"JSON parsing failed: {ex.Message}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An unexpected error occurred: {ex.Message}");
			}
		}

		public static bool IsGoodFishing(float wind, int cloudVis, int humidity, DateTime sun, int month)
		{
			// logic to determine fishing. Not accurate
			bool isGoodFishing = wind < 9 && cloudVis > 70 && humidity >= 40 && humidity <= 90 && IsDaytime(sun) && (month > 5 || month <= 9);

			return isGoodFishing;
		}

		public static bool HighWinds(float wind)
		{
			if (wind > 25)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		private static bool IsDaytime(DateTime time)
		{
			DateTime sunriseStart = time.Date.AddHours(6); // Assume sunrise starts at 6 AM
			DateTime sunsetEnd = time.Date.AddHours(18);  // Assume sunset ends at 6 PM

			return time >= sunriseStart && time <= sunsetEnd;
		}

	}
}
