using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Infrastructure.Data;

public class FormBuilderDbContext : IdentityDbContext<User>
{
    // Nullable so unit tests that construct the context without a full
    // service graph (via the (options) constructor) still work; when null,
    // the tenant filter returns everything (test-friendly). Production
    // always registers ICurrentUserService in DI.
    private readonly ICurrentUserService? _currentUser;

    // EF Core global query filters can't call methods on nullable references
    // (the expression translator evaluates the whole tree, not just the
    // reachable branch of a short-circuit). Reading the orgId through this
    // property means the filter body is a plain nullable-Guid comparison —
    // no method calls, no null-deref during translation.
    private Guid? CurrentOrgId => _currentUser?.GetOrganizationIdOrNull();

    public FormBuilderDbContext(DbContextOptions<FormBuilderDbContext> options) : base(options)
    {
    }

    public FormBuilderDbContext(DbContextOptions<FormBuilderDbContext> options, ICurrentUserService currentUser) : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Form> Forms { get; set; }
    public DbSet<FormVersion> FormVersions { get; set; }
    public DbSet<FormVersionField> FormVersionFields { get; set; }
    public DbSet<FormSubmission> FormSubmissions { get; set; }
    public DbSet<FormSubmissionValue> FormSubmissionValues { get; set; }
    public DbSet<FormSubmissionDraft> FormSubmissionDrafts { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<Organization> Organizations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(64);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Form>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrganizationId);
            // Global tenant filter — every `_context.Forms` query is
            // silently constrained to the current caller's org. Public
            // slug endpoints must bypass with .IgnoreQueryFilters().
            entity.HasQueryFilter(f => CurrentOrgId == null || f.OrganizationId == CurrentOrgId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(16);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.BrandColor).HasMaxLength(9);
            entity.Property(e => e.AccessPassword).HasMaxLength(200);
            entity.Property(e => e.ThankYouMessage).HasMaxLength(2000);
            entity.Property(e => e.RedirectUrl).HasMaxLength(500);
            entity.Property(e => e.WebhookUrl).HasMaxLength(500);
            entity.Property(e => e.WebhookSecret).HasMaxLength(128);
            entity.Property(e => e.ConfirmationEmailSubject).HasMaxLength(200);
            entity.Property(e => e.ConfirmationEmailBody).HasMaxLength(4000);
            entity.Property(e => e.Locale).HasMaxLength(10);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.HasMany(e => e.Versions)
                  .WithOne(e => e.Form)
                  .HasForeignKey(e => e.FormId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FormVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.HasMany(e => e.Fields)
                  .WithOne(e => e.FormVersion)
                  .HasForeignKey(e => e.FormVersionId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.FormId, e.VersionNumber }).IsUnique();
            // Tenant scope by joining to the parent Form. Adds a join on
            // every version query; acceptable for MVP correctness. Public
            // slug flows that need cross-tenant access use IgnoreQueryFilters.
            entity.HasQueryFilter(v => CurrentOrgId == null || v.Form.OrganizationId == CurrentOrgId);
        });

        modelBuilder.Entity<FormVersionField>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Label).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Validation).HasMaxLength(500);
            entity.Property(e => e.DefaultValue).HasMaxLength(500);
            entity.Property(e => e.Options).HasMaxLength(2000);
            entity.Property(e => e.Placeholder).HasMaxLength(200);
            entity.Property(e => e.HelpText).HasMaxLength(500);
            entity.Property(e => e.ShowIfCondition).HasMaxLength(500);
        });

        modelBuilder.Entity<FormSubmission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SubmittedAt).IsRequired();
            entity.Property(e => e.SubmitterName).HasMaxLength(200);
            entity.Property(e => e.SubmitterEmail).HasMaxLength(200);
            entity.Property(e => e.SubmitterIpAddress).HasMaxLength(45);
            entity.Property(e => e.AdminNotes).HasMaxLength(4000);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.HasOne(e => e.FormVersion)
                  .WithMany()
                  .HasForeignKey(e => e.FormVersionId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Values)
                  .WithOne(e => e.FormSubmission)
                  .HasForeignKey(e => e.FormSubmissionId)
                  .OnDelete(DeleteBehavior.Cascade);
            // Public submission POSTs (anonymous /f/:slug flow) construct
            // the submission via IgnoreQueryFilters()-based lookups on the
            // slug path. Authenticated reads/writes go through this filter.
            entity.HasQueryFilter(s => CurrentOrgId == null || s.FormVersion.Form.OrganizationId == CurrentOrgId);
        });

        modelBuilder.Entity<FormSubmissionValue>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FieldName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FieldValue).HasMaxLength(2000);
        });

        modelBuilder.Entity<FormSubmissionDraft>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ResumeToken).IsUnique();
            entity.HasIndex(e => e.ExpiresAt);
            entity.Property(e => e.SubmitterName).HasMaxLength(200);
            entity.Property(e => e.SubmitterEmail).HasMaxLength(200);
            entity.Property(e => e.FieldValuesJson).IsRequired().HasMaxLength(64000);
            entity.HasOne(e => e.Form)
                  .WithMany()
                  .HasForeignKey(e => e.FormId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrganizationId);
            // Same tenant filter pattern as Form. The API-key auth handler
            // needs to look up ANY key by its hash to authenticate — that
            // lookup runs before ICurrentUserService is populated, so it
            // must use .IgnoreQueryFilters().
            entity.HasQueryFilter(k => CurrentOrgId == null || k.OrganizationId == CurrentOrgId);
            // KeyHash is a fixed-length SHA-256 hex string; the unique index
            // is what makes lookup on incoming requests cheap.
            entity.HasIndex(e => e.KeyHash).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.KeyPrefix).IsRequired().HasMaxLength(16);
            entity.Property(e => e.KeyHash).IsRequired().HasMaxLength(128);
            entity.Property(e => e.CreatedAt).IsRequired();
        });
    }
}
