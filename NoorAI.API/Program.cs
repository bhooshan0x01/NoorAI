using Microsoft.EntityFrameworkCore;
using NoorAI.API.Data;
using NoorAI.API.Repositories;
using NoorAI.API.Repositories.Interfaces;
using NoorAI.API.Services;
using NoorAI.API.Services.Interfaces;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
        builder.WithOrigins("http://localhost:3001", "https://localhost:3001")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials());
});

// Add Ollama service
builder.Services.AddHttpClient<IOllamaService, OllamaService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
});
builder.Services.AddScoped<IOllamaService, OllamaService>();

// Add other services
builder.Services.AddScoped<IUploadService, UploadService>();
builder.Services.AddScoped<IInterviewRepository, InterviewRepository>();
builder.Services.AddScoped<IInterviewService, InterviewService>();

// Add database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
    // HTTP
    options.ListenLocalhost(5158);
    
    // HTTPS
    options.ListenLocalhost(5159, listenOptions =>
    {
        listenOptions.UseHttps();
    });

    // Configure timeouts
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Comment out HTTPS redirection for local development
// app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
