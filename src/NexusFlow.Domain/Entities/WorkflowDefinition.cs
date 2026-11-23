using NexusFlow.Domain.Common;
using NexusFlow.Domain.Exceptions;

namespace NexusFlow.Domain.Entities;

public class WorkflowDefinition : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int Version { get; private set; } = 1;
    public bool IsActive { get; private set; } = true;
    public string CreatedBy { get; private set; } = string.Empty;

    private readonly List<WorkflowStep> _steps = new();
    public IReadOnlyCollection<WorkflowStep> Steps => _steps.AsReadOnly();

    protected WorkflowDefinition() { }

    public WorkflowDefinition(string name, string description, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Workflow name cannot be empty.");

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "System" : createdBy.Trim();
    }

    public WorkflowStep AddStep(int order, string name, Enums.StepType type, string configJson, int retries = 3, int timeoutSec = 30)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Step name cannot be empty.");

        var step = new WorkflowStep(Id, order, name, type, configJson, retries, timeoutSec);
        _steps.Add(step);
        SetUpdated();
        return step;
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdated();
    }

    public void IncrementVersion()
    {
        Version++;
        SetUpdated();
    }
}
