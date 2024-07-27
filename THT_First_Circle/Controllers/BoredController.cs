using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Diagnostics;

namespace THT_First_Circle.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableRateLimiting("fixed")]

    public class BoredController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public BoredController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        #region Get
        /// <summary>
        /// Gets the list of json on external bored-api.
        /// </summary>
        /// <param name="times">The api times identifier.</param>
        /// <param name="format">The type of return.</param>
        /// <returns></returns>
        [HttpGet("get-app-brewery")]
        public async Task<IActionResult> GetList(int times, string format)
        {
            if (times <= 0)
            {
                return BadRequest("The number of times must be greater than zero.");
            }

            var api = "https://bored-api.appbrewery.com/random";
            var results = new List<Bored>();

            for (int i = 0; i < times; i++)
            {
                try
                {
                    var response = await _httpClient.GetStringAsync(api);
                    var boredItem = JsonSerializer.Deserialize<Bored>(response);
                    if (boredItem != null)
                    {
                        results.Add(boredItem);
                    }
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, "Internal server error");
                }
            }

            if (format == "json")
            {
                var jsonContent = JsonSerializer.Serialize(results);
                return Content(jsonContent, "application/json");
            }
            else if (format == "csv")
            {
                var csvContent = ConvertToCsv(results);
                return File(Encoding.UTF8.GetBytes(csvContent), "text/csv", "results.csv");
            }
            else if (format == "console")
            {
                var consoleContent = string.Join(Environment.NewLine, results.Select(r => JsonSerializer.Serialize(r)));
                Debug.WriteLine(consoleContent);
                return Ok(consoleContent);
            }
            else
            {
                return BadRequest("Unsupported format. Supported formats are 'json', 'csv', and 'console'.");
            }
        }
        #endregion Get

        #region private
        /// <summary>
        /// Converts the list into a comma seperated values.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string ConvertToCsv(List<Bored> data)
        {
            var sb = new StringBuilder();
            foreach (var item in data)
            {
                sb.AppendLine($"{item.activity},{item.availability},{item.type},{item.participants},{item.price},{item.accessibility},{item.duration},{item.kidFriendly},{item.link},{item.key}");
            }
            return sb.ToString();
        }
        #endregion private
    }
}
