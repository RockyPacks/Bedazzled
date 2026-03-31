using Bedazzled.Application.Interfaces;
using Bedazzled.Application.Services;
using Bedazzled.Api.Services;
using Bedazzled.Infrastructure.Repositories;
using Google.Cloud.Firestore;
using Resend;

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
        GoogleCredential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(keyPath)
    }.Build();
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Dependency Injection
builder.Services.AddScoped<IBookingRepository, FirestoreBookingRepository>();
builder.Services.AddScoped<IContactRepository, FirestoreContactRepository>();
builder.Services.AddScoped<IReviewRepository, FirestoreReviewRepository>();
builder.Services.AddHttpClient<IFirebaseAdminAuthService, FirebaseAdminAuthService>();

// Resend Setup
var resendApiKey = builder.Configuration["Resend:ApiKey"]
    ?? throw new InvalidOperationException("Resend:ApiKey is not configured. Set it with user-secrets or the Resend__ApiKey environment variable.");

builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(o =>
{
    o.ApiToken = resendApiKey;
});
builder.Services.AddTransient<IResend, ResendClient>();

builder.Services.AddScoped<IEmailService, EmailService>();
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
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
}

// app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapGet("/api/health", () => Results.Ok(new { Status = "Healthy", Time = DateTime.Now }));
app.MapControllers();

app.Run();
