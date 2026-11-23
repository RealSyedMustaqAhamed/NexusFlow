using Microsoft.EntityFrameworkCore;
using NexusFlow.Domain.Entities;
using NexusFlow.Domain.Repositories;

namespace NexusFlow.Infrastructure.Persistence.Repositories;

public class WorkflowRepository : IWorkflowRepository
{
    private readonly NexusDbContext _context;

    public WorkflowRepository(NexusDbContext context)
    {
        _context = context;
    }

    public async Task<WorkflowDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Workflows
            .Include(w => w.Steps)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkflowDefinition>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Workflows
            .Include(w => w.Steps)
            .Where(w => w.IsActive)
            .OrderByDescending(w => w.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(WorkflowDefinition workflow, CancellationToken cancellationToken = default)
    {
        await _context.Workflows.AddAsync(workflow, cancellationToken);
    }

    public void Update(WorkflowDefinition workflow)
    {
        _context.Workflows.Update(workflow);
    }
}

public class ExecutionRepository : IExecutionRepository
{
    private readonly NexusDbContext _context;

    public ExecutionRepository(NexusDbContext context)
    {
        _context = context;
    }

    public async Task<WorkflowExecution?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.WorkflowExecutions
            .Include(e => e.StepExecutions)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkflowExecution>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default)
    {
        return await _context.WorkflowExecutions
            .Include(e => e.StepExecutions)
            .Where(e => e.WorkflowDefinitionId == workflowId)
            .OrderByDescending(e => e.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkflowExecution>> GetPendingExecutionsAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        return await _context.WorkflowExecutions
            .Include(e => e.StepExecutions)
            .Where(e => e.Status == Domain.Enums.ExecutionStatus.Pending)
            .OrderBy(e => e.CreatedAtUtc)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(WorkflowExecution execution, CancellationToken cancellationToken = default)
    {
        await _context.WorkflowExecutions.AddAsync(execution, cancellationToken);
    }

    public void Update(WorkflowExecution execution)
    {
        _context.WorkflowExecutions.Update(execution);
    }
}

public class IncidentTriageRepository : IIncidentTriageRepository
{
    private readonly NexusDbContext _context;

    public IncidentTriageRepository(NexusDbContext context)
    {
        _context = context;
    }

    public async Task<IncidentTriage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.IncidentTriages.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IncidentTriage?> GetByExecutionIdAsync(Guid executionId, CancellationToken cancellationToken = default)
    {
        return await _context.IncidentTriages.FirstOrDefaultAsync(t => t.WorkflowExecutionId == executionId, cancellationToken);
    }

    public async Task<IReadOnlyList<IncidentTriage>> GetRecentTriagesAsync(int limit = 20, CancellationToken cancellationToken = default)
    {
        return await _context.IncidentTriages
            .OrderByDescending(t => t.CreatedAtUtc)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(IncidentTriage triage, CancellationToken cancellationToken = default)
    {
        await _context.IncidentTriages.AddAsync(triage, cancellationToken);
    }

    public void Update(IncidentTriage triage)
    {
        _context.IncidentTriages.Update(triage);
    }
}
