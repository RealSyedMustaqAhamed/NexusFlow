using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NexusFlow.Application.Common.Interfaces;
using NexusFlow.Domain.Repositories;
using NexusFlow.Infrastructure.BackgroundJobs;
using NexusFlow.Infrastructure.Persistence;
using NexusFlow.Infrastructure.Persistence.Repositories;
using NexusFlow.Infrastructure.Services;
using Polly;

namespace NexusFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=nexusflow.db";

        services.AddDbContext<NexusDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<NexusDbContext>());
        services.AddScoped<IWorkflowRepository, WorkflowRepository>();
        services.AddScoped<IExecutionRepository, ExecutionRepository>();
        services.AddScoped<IIncidentTriageRepository, IncidentTriageRepository>();

        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddScoped<IAiTriageAgentService, AiTriageAgentService>();

        services.AddHttpClient<IWebhookDispatcher, WebhookDispatcher>()
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.BackoffType = DelayBackoffType.Exponential;
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            });

        services.AddHostedService<WorkflowExecutionBackgroundWorker>();

        return services;
    }
}
