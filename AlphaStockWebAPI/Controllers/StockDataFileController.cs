// using Dapper;
// using Microsoft.Data.SqlClient;
// using AlphaStockWebAPI.Models;
// using Microsoft.AspNetCore.Mvc;
// using System.Globalization;
// using System.Net.Http;
// using System.Text.Json;
// using System.Threading.Tasks;

// namespace AlphaStockWebAPI.Controllers
// {
//     [ApiController]
//     [Route("[controller]")]
//     public class StockDataController : ControllerBase
//     {
//         private readonly IHttpClientFactory _httpClientFactory;
//         private readonly string _connectionString = "DefaultConnection";

//         public StockDataController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
//         {
//             _httpClientFactory = httpClientFactory;
//             _connectionString = configuration.GetConnectionString("DefaultConnection")!;
//         }


//         private async Task InsertStockDataFromFileAsync()
//         {
//             var json = System.IO.File.ReadAllText("AlphaStockdataexample.json");
//             var root = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
//             var symbol = root!["Meta Data"].GetProperty("2. Symbol").GetString();
//             var timeSeries = root!["Time Series (Daily)"];

//             using var connection = new SqlConnection(_connectionString);

//             foreach (var dateProp in timeSeries.EnumerateObject())
//             {
//                 var date = DateTime.ParseExact(dateProp.Name, "yyyy-MM-dd", CultureInfo.InvariantCulture);
//                 var values = dateProp.Value;

//                 var sql = @"INSERT INTO AlphaStockSchema.TimeSeriesDaily
//                             (Date, Symbol, [Open], High, Low, [Close], Volume)
//                             VALUES (@Date, @Symbol, @Open, @High, @Low, @Close, @Volume)";

//                 await connection.ExecuteAsync(sql, new
//                 {
//                     Date = date,
//                     Symbol = symbol,
//                     Open = decimal.Parse(values.GetProperty("1. open").GetString()!, CultureInfo.InvariantCulture),
//                     High = decimal.Parse(values.GetProperty("2. high").GetString()!, CultureInfo.InvariantCulture),
//                     Low = decimal.Parse(values.GetProperty("3. low").GetString()!, CultureInfo.InvariantCulture),
//                     Close = decimal.Parse(values.GetProperty("4. close").GetString()!, CultureInfo.InvariantCulture),
//                     Volume = long.Parse(values.GetProperty("5. volume").GetString()!, CultureInfo.InvariantCulture)
//                 });
//             }
//         }

//         [HttpPost("import-from-file")]
//         public async Task<IActionResult> ImportFromFile()
//         {
//             await InsertStockDataFromFileAsync();
//             return Ok("Data imported from AlphaStockdataexample.json.");
//         }
//     }
// }