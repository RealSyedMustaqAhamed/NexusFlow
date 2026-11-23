using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NexusFlow.Domain.Repositories;

namespace NexusFlow.Infrastructure.BackgroundJobs;

public class WorkflowExecutionBackgroundWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WorkflowExecutionBackgroundWorker> _logger;

    public WorkflowExecutionBackgroundWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<WorkflowExecutionBackgroundWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NexusFlow Execution Background Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var executionRepo = scope.ServiceProvider.GetRequiredService<IExecutionRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var pendingExecutions = await executionRepo.GetPendingExecutionsAsync(5, stoppingToken);

                foreach (var execution in pendingExecutions)
                {
                    _logger.LogInformation("Worker processing execution ID: {Id}", execution.Id);
                    execution.Start();

                    foreach (var stepExec in execution.StepExecutions)
                    {
                        stepExec.Start();
                        // Simulate step work execution
                        stepExec.Complete(outputData: "{\"status\": \"step_success\", \"processedAtUtc\": \"" + DateTime.UtcNow.ToString("o") + "\"}");
                    }

                    execution.Complete(outputJson: "{\"status\": \"workflow_completed\", \"executionEngine\": \"NexusFlow.v1\"}");
                    executionRepo.Update(execution);
                }

                if (pendingExecutions.Count > 0)
                {
                    await unitOfWork.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during execution worker loop.");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }

        _logger.LogInformation("NexusFlow Execution Background Worker stopped.");
    }
}
