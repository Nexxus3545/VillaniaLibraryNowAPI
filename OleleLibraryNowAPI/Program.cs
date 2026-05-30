using OleleLibraryNowAPI.Repositories;
using OleleLibraryNowAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IBookRepository, InMemoryBookRepository>();
builder.Services.AddSingleton<IBookImportService, LegacyBookImportService>();

var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://+:{port}");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapGet("/", () => Results.Redirect("/api/v1/books"));
app.MapControllers();

if (app.Configuration.GetValue("ETL:AutoImportOnStartup", false))
{
    using var scope = app.Services.CreateScope();
    var importer = scope.ServiceProvider.GetRequiredService<IBookImportService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("StartupImport");

    try
    {
        var summary = importer.Import();
        logger.LogInformation(
            "Imported {LoadedRows} books from {SourceFile} during startup.",
            summary.LoadedRows,
            summary.SourceFile);
    }
    catch (FileNotFoundException exception)
    {
        logger.LogWarning(exception, "Startup ETL skipped because the legacy CSV file was not found.");
    }
    catch (InvalidDataException exception)
    {
        logger.LogWarning(exception, "Startup ETL skipped because the legacy CSV file was invalid.");
    }
}

await app.RunAsync();
