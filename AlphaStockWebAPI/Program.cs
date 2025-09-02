using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Add an HttpClient service to be used for making HTTP requests.
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// This middleware sets the default file to index.html.
app.UseDefaultFiles();
// This middleware serves static files from the wwwroot folder.
app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// // Define an API endpoint at "/stockdata" that clients can call to get data.
// app.MapGet("/stockdata", async (HttpClient client) =>
// {
//     // The URL for the Alpha Vantage API.
//     // replace the "demo" apikey below with your own key from https://www.alphavantage.co/support/#api-key
//     string QUERY_URL = "https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol=IBM&apikey=8U3OYRJ0NJE68PFA";

//     try
//     {
//         // Asynchronously get the JSON data as a string from the Alpha Vantage API.
//         string jsonString = await client.GetStringAsync(QUERY_URL);
//         // Deserialize the JSON string into a format that can be easily returned.
//         var data = JsonSerializer.Deserialize<object>(jsonString);
//         // Return the data with a 200 OK status code.
//         return Results.Ok(data);
//     }
//     catch
//     {
//         // If an error occurs, return a 500 Internal Server Error with a message.
//         return Results.Problem("An error occurred while fetching data from Alpha Vantage.", statusCode: 500);
//     }
// });

app.MapControllers();

app.Run();

