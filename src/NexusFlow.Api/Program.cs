using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NexusFlow.Api.Middleware;
using NexusFlow.Application;
using NexusFlow.Infrastructure;
using NexusFlow.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add Application & Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add Controllers and JSON configuration
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Health Checks
builder.Services.AddHealthChecks();

// JWT Authentication
var jwtSecret = builder.Configuration["JwtSettings:Secret"] ?? "NexusFlow_Super_Secure_Secret_Key_For_Jwt_Generation_2026!";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "NexusFlow",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "NexusFlowClients",
        ClockSkew = TimeSpan.Zero
    };
});

// Swagger / OpenAPI with JWT Bearer Definition
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NexusFlow Enterprise Workflow & AI Orchestration API",
        Version = "v1",
        Description = "Event-driven workflow orchestration engine with resilient Polly integration and Agentic AI Incident Triage."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Ensure Database Created and Seed initial sample workflow
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NexusDbContext>();
    db.Database.EnsureCreated();

    if (!db.Workflows.Any())
    {
        var sampleWf = new NexusFlow.Domain.Entities.WorkflowDefinition(
            "ServiceNow-Incident-Triage-Pipeline",
            "Automated triage workflow that extracts incident telemetry, runs Agentic AI root-cause analysis, and dispatches webhooks.",
            "Syed Mustaq Ahamed");

        sampleWf.AddStep(1, "ExtractIncidentMetadata", NexusFlow.Domain.Enums.StepType.DataTransform, "{\"targetField\": \"telemetry\"}");
        sampleWf.AddStep(2, "AiRootCauseAnalysis", NexusFlow.Domain.Enums.StepType.AiAgentTriage, "{\"model\": \"gemini-1.5-pro\", \"confidenceThreshold\": 0.85}");
        sampleWf.AddStep(3, "DispatchRemediationWebhook", NexusFlow.Domain.Enums.StepType.Webhook, "{\"endpoint\": \"https://api.internal/remediation\", \"retries\": 3}");

        db.Workflows.Add(sampleWf);
        db.SaveChanges();
    }
}

// Global Exception Handler Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NexusFlow API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }
