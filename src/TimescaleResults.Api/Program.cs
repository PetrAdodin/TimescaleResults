using Microsoft.EntityFrameworkCore;
using TimescaleResults.Api.Csv;
using TimescaleResults.Api.Data;
using TimescaleResults.Api.ErrorHandling;
using TimescaleResults.Api.Services;
using TimescaleResults.Api.Statistics;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "Connection string 'Postgres' was not found.");

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<CsvFileParser>();
builder.Services.AddScoped<CsvValueValidator>();
builder.Services.AddScoped<StatisticsCalculator>();
builder.Services.AddScoped<CsvUploadService>();
builder.Services.AddScoped<ResultQueryService>();
builder.Services.AddScoped<ValueQueryService>();

builder.Services.AddExceptionHandler<CsvValidationExceptionHandler>();
builder.Services.AddExceptionHandler<ResultFilterValidationExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "TimescaleResults API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();