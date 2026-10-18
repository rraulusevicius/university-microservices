using Microsoft.EntityFrameworkCore;
using CoursesAPI.Data;
using CoursesAPI.Services;
using CoursesAPI.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using MassTransit;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "CoursesAPI")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Seq(Environment.GetEnvironmentVariable("Seq__Uri") ?? "http://seq-individual:5341")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddDbContext<CoursesAPIContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CoursesApiContext")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health checks - SQL Server + RabbitMQ (custom 7.x check)
var sqlConn = builder.Configuration.GetConnectionString("CoursesApiContext")!;
var rabbitUri = builder.Configuration["RabbitMQ:Uri"]
    ?? throw new InvalidOperationException("RabbitMQ:Uri is not configured (set RabbitMQ__Uri)");
builder.Services.AddHealthChecks()
    .AddSqlServer(sqlConn, name: "sqlserver", tags: new[] { "db", "sql" })
    .AddCheck("rabbitmq", new RabbitMQHealthCheck(rabbitUri), tags: new[] { "messaging" });

// Lab 2.4 - RabbitMQ 7.x Publisher
builder.Services.AddSingleton<IRabbitMQPublisher>(
    new RabbitMQPublisher(rabbitUri, "enrollment_exchange"));

// MassTransit publisher
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(rabbitUri));
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Health check endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseAuthorization();
app.MapControllers();
app.Run();
