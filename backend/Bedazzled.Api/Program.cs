using Bedazzled.Application.Interfaces;
using Bedazzled.Application.Services;
using Bedazzled.Infrastructure.Repositories;
using Google.Cloud.Firestore;

var builder = WebApplication.CreateBuilder(args);

// Firebase Setup
var firebaseConfig = builder.Configuration.GetSection("Firebase");
string projectId = firebaseConfig["ProjectId"] ?? throw new InvalidOperationException("Firebase ProjectId is not configured.");
string keyPath = Path.Combine(builder.Environment.ContentRootPath, firebaseConfig["KeyPath"] ?? "firebase-key.json");

builder.Services.AddSingleton(s => 
{
    return new FirestoreDbBuilder
    {
        ProjectId = projectId,
        CredentialsPath = keyPath
    }.Build();
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Dependency Injection
builder.Services.AddScoped<IBookingRepository, FirestoreBookingRepository>();
builder.Services.AddScoped<IContactRepository, FirestoreContactRepository>();
builder.Services.AddScoped<IReviewRepository, FirestoreReviewRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        b => b.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapGet("/api/health", () => Results.Ok(new { Status = "Healthy", Time = DateTime.Now }));
app.MapControllers();

app.Run();


