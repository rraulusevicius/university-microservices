using Microsoft.EntityFrameworkCore;
using StudentsAPI;
using StudentsAPI.Data;
using StudentsAPI.HealthChecks;
using System.Text.Json.Serialization;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using StudentsAPI.GrpcServices;
using MassTransit;
using StudentsAPI.Consumers;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "StudentsAPI")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Seq(Environment.GetEnvironmentVariable("Seq__Uri") ?? "http://seq-individual:5341")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddDbContext<StudentsAPIContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StudentsAPIContext")));

builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddGrpc();

// Health checks - SQL Server + RabbitMQ (custom 7.x check)
var sqlConn = builder.Configuration.GetConnectionString("StudentsAPIContext")!;
var rabbitUri = builder.Configuration["RabbitMQ:Uri"]
    ?? throw new InvalidOperationException("RabbitMQ:Uri is not configured (set RabbitMQ__Uri)");
builder.Services.AddHealthChecks()
    .AddSqlServer(sqlConn, name: "sqlserver", tags: new[] { "db", "sql" })
    .AddCheck("rabbitmq", new RabbitMQHealthCheck(rabbitUri), tags: new[] { "messaging" });

// Lab 2.4 - RabbitMQ 7.x Subscriber
builder.Services.AddHostedService<EnrollmentDataCollector>();

// MassTransit consumer
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CourseCreatedConsumer>();

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

app.MapGrpcService<StudentsAPI.GrpcServices.StudentGrpcService>();
app.UseAuthorization();
app.MapControllers();
app.Run();
