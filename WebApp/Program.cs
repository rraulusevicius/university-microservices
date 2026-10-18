using WebApp.Services;
using Serilog;
using StudentsAPI.Protos;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "WebApp")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Seq(Environment.GetEnvironmentVariable("Seq__Uri") ?? "http://seq-individual:5341")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddControllersWithViews();

// Register HttpClient for each microservice — talks directly to each service
var studentsApiUrl = builder.Configuration["Services:StudentsAPI"] ?? "http://studentsapi-individual:8080";
var coursesApiUrl = builder.Configuration["Services:CoursesAPI"] ?? "http://coursesapi-individual:8080";

// gRPC client - direct communication to StudentsAPI via gRPC
var studentsGrpcUrl = builder.Configuration["Services:StudentsGrpc"] ?? "http://studentsapi-individual:5001";
builder.Services.AddGrpcClient<StudentGrpc.StudentGrpcClient>(o =>
{
    o.Address = new Uri(studentsGrpcUrl);
});

builder.Services.AddHttpClient<StudentService>(c => c.BaseAddress = new Uri(studentsApiUrl));
builder.Services.AddHttpClient<CourseService>(c => c.BaseAddress = new Uri(coursesApiUrl));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();