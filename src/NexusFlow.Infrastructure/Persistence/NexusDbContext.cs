using Microsoft.EntityFrameworkCore;
using NexusFlow.Domain.Entities;
using NexusFlow.Domain.Repositories;

namespace NexusFlow.Infrastructure.Persistence;

public class NexusDbContext : DbContext, IUnitOfWork
{
    public DbSet<WorkflowDefinition> Workflows => Set<WorkflowDefinition>();
    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
    public DbSet<WorkflowExecution> WorkflowExecutions => Set<WorkflowExecution>();
    public DbSet<StepExecution> StepExecutions => Set<StepExecution>();
    public DbSet<IncidentTriage> IncidentTriages => Set<IncidentTriage>();

    public NexusDbContext(DbContextOptions<NexusDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WorkflowDefinition>(b =>
        {
            b.HasKey(w => w.Id);
            b.Property(w => w.Name).IsRequired().HasMaxLength(150);
            b.Property(w => w.Description).HasMaxLength(500);
            b.Property(w => w.CreatedBy).IsRequired().HasMaxLength(100);
            b.HasMany(w => w.Steps)
             .WithOne()
             .HasForeignKey(s => s.WorkflowDefinitionId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkflowStep>(b =>
        {
            b.HasKey(s => s.Id);
            b.Property(s => s.Name).IsRequired().HasMaxLength(100);
            b.Property(s => s.ConfigurationJson).IsRequired();
        });

        modelBuilder.Entity<WorkflowExecution>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.TriggerSource).IsRequired().HasMaxLength(50);
            b.Property(e => e.InputPayloadJson).IsRequired();
            b.HasMany(e => e.StepExecutions)
             .WithOne()
             .HasForeignKey(s => s.WorkflowExecutionId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StepExecution>(b =>
        {
            b.HasKey(s => s.Id);
        });

        modelBuilder.Entity<IncidentTriage>(b =>
        {
            b.HasKey(t => t.Id);
            b.Property(t => t.IncidentKey).IsRequired().HasMaxLength(50);
            b.Property(t => t.RootCauseCategory).IsRequired().HasMaxLength(100);
            b.Property(t => t.AiSummary).IsRequired().HasMaxLength(1000);
            b.Property(t => t.RecommendedAction).IsRequired().HasMaxLength(1000);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
