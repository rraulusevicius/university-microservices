using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "ApiGateway")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Seq(Environment.GetEnvironmentVariable("Seq__Uri") ?? "http://seq:5341")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("ocelot.json");

builder.Services.AddOcelot();

// Health checks - monitor downstream services
var studentsUrl = Environment.GetEnvironmentVariable("HealthChecks__StudentsAPI") ?? "http://studentsapi-individual:8080";
var coursesUrl = Environment.GetEnvironmentVariable("HealthChecks__CoursesAPI") ?? "http://coursesapi-individual:8080";
var gatewayUrl = Environment.GetEnvironmentVariable("HealthChecks__ApiGateway") ?? "http://apigateway-individual:8080";

builder.Services.AddHealthChecks()
    .AddUrlGroup(new Uri($"{studentsUrl}/health"), name: "studentsapi", tags: new[] { "api" })
    .AddUrlGroup(new Uri($"{coursesUrl}/health"), name: "coursesapi", tags: new[] { "api" });

builder.Services.AddHealthChecksUI(options =>
{
    options.SetEvaluationTimeInSeconds(30);
    options.MaximumHistoryEntriesPerEndpoint(50);
    options.AddHealthCheckEndpoint("StudentsAPI", $"{studentsUrl}/health");
    options.AddHealthCheckEndpoint("CoursesAPI", $"{coursesUrl}/health");
    options.AddHealthCheckEndpoint("API Gateway", $"{gatewayUrl}/health");
}).AddInMemoryStorage();

var app = builder.Build();

// Must use explicit routing so health endpoints are handled BEFORE Ocelot intercepts
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    endpoints.MapHealthChecksUI(options =>
    {
        options.UIPath = "/health-ui";
    });
});

app.UseOcelot().Wait();

app.Run();
