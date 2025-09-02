using Dapper;
using Microsoft.Data.SqlClient;
using AlphaStockWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Numerics;

namespace AlphaStockWebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StockDataController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _connectionString = "DefaultConnection";

        public StockDataController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        [HttpGet]
        public async Task<IActionResult> GetStockData([FromQuery] string symbol = "IBM")
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return BadRequest("Symbol is required.");

            using SqlConnection connection = new SqlConnection(_connectionString);

            // 1. Try to get the latest data from your DB
            string latestDbDataCheck = @"SELECT * FROM AlphaStockSchema.TimeSeriesDaily WHERE Symbol = @Symbol AND [Date] = '2025-08-29'";
            // CAST(DATEADD(DAY, -1, GETDATE()) AS DATE)
            List<TimeSeriesDaily> dbCheckResult = (await connection.QueryAsync<TimeSeriesDaily>(latestDbDataCheck, new { Symbol = symbol })).ToList();

            string sql = @"SELECT * FROM AlphaStockSchema.TimeSeriesDaily WHERE Symbol = @Symbol";

            if (dbCheckResult.Any())
            {
                List<TimeSeriesDaily> dbResult = (await connection.QueryAsync<TimeSeriesDaily>(sql, new { Symbol = symbol })).ToList();

                // After you have dbResult sorted by date descending:
                for (int i = 1; i < dbResult.Count; i++)
                {
                    decimal prevClose = dbResult[i - 1].Close;
                    decimal currClose = dbResult[i].Close;
                    if (prevClose != 0)
                        dbResult[i].DayOnDayChangePercent = ((currClose - prevClose) / prevClose) * 100;
                }

                for (int i = 1; i < dbResult.Count; i++)
                {
                    decimal currOpen = dbResult[i].Open;
                    decimal currClose = dbResult[i].Close;
                    dbResult[i].ChangeFromOpenPercent = ((currClose - currOpen) / currOpen) * 100;
                }

                dbResult.Reverse();
                // change
                decimal averageDbVolume = 0;
                for (int i = 0; i < 10; i++)
                {
                    averageDbVolume += dbResult[i].Volume;
                }
                averageDbVolume /= 10;
                dbResult[0].averageDbVolume = averageDbVolume;

                DateTime latestDateDb = dbResult.Max(r => r.Date);
                DateTime targetDateDb = latestDateDb.AddMonths(-1);

                TimeSeriesDaily? latestRecordDb = dbResult.Where(r => r.Date <= latestDateDb).FirstOrDefault();

                TimeSeriesDaily? oneMonthAgoRecordDb = dbResult
                    .Where(r => r.Date <= targetDateDb)
                    .OrderByDescending(r => r.Date)
                    .FirstOrDefault();
                int indexDb = 0;
                dbResult[indexDb].OneMonthTrailing = ((latestRecordDb!.Close - oneMonthAgoRecordDb!.Open) / oneMonthAgoRecordDb!.Open) * 100;

                Dictionary<string, Dictionary<string, object>> timeSeries = dbResult.ToDictionary(
                    r => r.Date.ToString("yyyy-MM-dd"),
                    r => new Dictionary<string, object>
                    {
                        ["1. open"] = r.Open,
                        ["2. high"] = r.High,
                        ["3. low"] = r.Low,
                        ["4. close"] = r.Close,
                        ["5. volume"] = r.Volume,
                        ["6. day_on_day_change_percent"] = r.DayOnDayChangePercent,
                        ["7. one_month_trailing"] = r.OneMonthTrailing,
                        ["8. change_from_open_percent"] = r.ChangeFromOpenPercent,
                        ["9. average_db_volume"] = r.averageDbVolume
                    }
                );
                Dictionary<string, object> result = new Dictionary<string, object>
                {
                    ["Meta Data"] = new Dictionary<string, string>
                    {
                        ["2. Symbol"] = symbol
                    },
                    ["Time Series (Daily)"] = timeSeries
                }; // the object in this dictionary is a time series data

                return Ok(result);
            }


            // 2. If not found, fetch from Alpha Vantage
            string QUERY_URL = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey=8U3OYRJ0NJE68PFA";
            HttpClient client = _httpClientFactory.CreateClient();
            string jsonString = await client.GetStringAsync(QUERY_URL);

            Dictionary<string, JsonElement>? root = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString);
            if (root == null || !root.ContainsKey("Time Series (Daily)"))
                return NotFound("No data found for symbol.");

            JsonElement timeSeriesAV = root["Time Series (Daily)"];
            List<TimeSeriesDaily> upsertList = new List<TimeSeriesDaily>();

            foreach (JsonProperty dateProp in timeSeriesAV.EnumerateObject())
            {
                DateTime date = DateTime.Parse(dateProp.Name);
                JsonElement values = dateProp.Value;

                TimeSeriesDaily record = new TimeSeriesDaily
                {
                    Date = date,
                    Symbol = symbol,
                    Open = decimal.Parse(values.GetProperty("1. open").GetString()!, CultureInfo.InvariantCulture),
                    High = decimal.Parse(values.GetProperty("2. high").GetString()!, CultureInfo.InvariantCulture),
                    Low = decimal.Parse(values.GetProperty("3. low").GetString()!, CultureInfo.InvariantCulture),
                    Close = decimal.Parse(values.GetProperty("4. close").GetString()!, CultureInfo.InvariantCulture),
                    Volume = long.Parse(values.GetProperty("5. volume").GetString()!, CultureInfo.InvariantCulture),
                };
                upsertList.Add(record);
            }


            // 3. Upsert into DB (MERGE or INSERT ... ON DUPLICATE KEY)
            foreach (TimeSeriesDaily rec in upsertList)
            {
                string upsertSql = @"
                    MERGE AlphaStockSchema.TimeSeriesDaily AS target
                    USING (SELECT @Date AS Date, @Symbol AS Symbol) AS source
                    ON (target.Date = source.Date AND target.Symbol = source.Symbol)
                    WHEN MATCHED THEN
                        UPDATE SET [Open] = @Open, High = @High, Low = @Low, [Close] = @Close, Volume = @Volume
                    WHEN NOT MATCHED THEN
                        INSERT (Date, Symbol, [Open], High, Low, [Close], Volume)
                        VALUES (@Date, @Symbol, @Open, @High, @Low, @Close, @Volume);";

                await connection.ExecuteAsync(upsertSql, rec);
            }


            // 4. Return the data to the UI (from DB)
            List<TimeSeriesDaily> resultFromDb = (await connection.QueryAsync<TimeSeriesDaily>(sql, new { Symbol = symbol })).ToList();

            // After you have resultFromDb sorted by date descending:
            for (int i = 1; i < resultFromDb.Count; i++)
            {
                var prevClose = resultFromDb[i - 1].Close;
                var currClose = resultFromDb[i].Close;
                if (prevClose != 0)
                    resultFromDb[i].DayOnDayChangePercent = ((currClose - prevClose) / prevClose) * 100;
            }

                for (int i = 1; i < resultFromDb.Count; i++)
                {
                    decimal currOpen = resultFromDb[i].Open;
                    decimal currClose = resultFromDb[i].Close;
                    resultFromDb[i].ChangeFromOpenPercent = ((currClose - currOpen) / currOpen) * 100;
                }

            resultFromDb.Reverse();

            DateTime latestDate = resultFromDb.Max(r => r.Date);
            DateTime targetDate = latestDate.AddMonths(-1);

            TimeSeriesDaily? latestRecord = resultFromDb.Where(r => r.Date <= latestDate).FirstOrDefault();

            TimeSeriesDaily? oneMonthAgoRecord = resultFromDb
                    .Where(r => r.Date <= targetDate)
                    .OrderByDescending(r => r.Date)
                    .FirstOrDefault();
            int index = 0;
            resultFromDb[index].OneMonthTrailing = ((latestRecord!.Close - oneMonthAgoRecord!.Open) / oneMonthAgoRecord!.Open) * 100;

            if (resultFromDb.Any())
            {
                Dictionary<string, Dictionary<string, decimal>> timeSeries = resultFromDb.ToDictionary(
                    r => r.Date.ToString("yyyy-MM-dd"),
                    r => new Dictionary<string, decimal>
                    {
                        ["1. open"] = r.Open,
                        ["2. high"] = r.High,
                        ["3. low"] = r.Low,
                        ["4. close"] = r.Close,
                        ["5. volume"] = r.Volume,
                        ["6. day_on_day_change_percent"] = r.DayOnDayChangePercent,
                        ["7. one_month_trailing"] = r.OneMonthTrailing,
                        ["8. change_from_open_percent"] = r.ChangeFromOpenPercent
                    }
                );
                Dictionary<string, object> result = new Dictionary<string, object>
                {
                    ["Meta Data"] = new Dictionary<string, string>
                    {
                        ["2. Symbol"] = symbol
                    },
                    ["Time Series (Daily)"] = timeSeries
                };
                return Ok(result);
            }
            throw new Exception("Alpha Vantage API call failed.");
        }
    }
}