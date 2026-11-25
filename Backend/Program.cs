
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Register API controllers.
builder.Services.AddControllers();

// PART 1: Telemetry and sensor services.
builder.Services.AddSingleton<SensorService>();
builder.Services.AddScoped<TelemetryPacketService>();
builder.Services.AddScoped<TelemetryAnalysisService>();
builder.Services.AddScoped<TelemetryGeneratorService>();
builder.Services.AddScoped<TelemetryArrayService>();

// PART 2: Queues, alerts, commands and search intelligence.
builder.Services.AddSingleton<TelemetryQueueService>();
builder.Services.AddSingleton<AlertPriorityQueueService>();
builder.Services.AddSingleton<SmartXOperationsService>();
builder.Services.AddSingleton<SearchIntelligenceService>();

// PART 3: Shared AVL tree historical telemetry index.
builder.Services.AddSingleton<TelemetryHistoryService>();
builder.Services.AddSingleton<MeshNetworkService>();

// Configure Swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Allow the React frontend to communicate with the API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Enable Swagger during development.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();

