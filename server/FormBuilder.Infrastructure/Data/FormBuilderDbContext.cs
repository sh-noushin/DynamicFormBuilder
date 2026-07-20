using FormBuilder.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Infrastructure.Data;

public class FormBuilderDbContext : IdentityDbContext<User>
{
    public FormBuilderDbContext(DbContextOptions<FormBuilderDbContext> options) : base(options)
    {
    }

    public DbSet<Form> Forms { get; set; }
    public DbSet<FormVersion> FormVersions { get; set; }
    public DbSet<FormVersionField> FormVersionFields { get; set; }
    public DbSet<FormSubmission> FormSubmissions { get; set; }
    public DbSet<FormSubmissionValue> FormSubmissionValues { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Form>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(16);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.BrandColor).HasMaxLength(9);
            entity.Property(e => e.AccessPassword).HasMaxLength(200);
            entity.Property(e => e.ThankYouMessage).HasMaxLength(2000);
            entity.Property(e => e.RedirectUrl).HasMaxLength(500);
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
            entity.HasOne(e => e.FormVersion)
                  .WithMany()
                  .HasForeignKey(e => e.FormVersionId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Values)
                  .WithOne(e => e.FormSubmission)
                  .HasForeignKey(e => e.FormSubmissionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FormSubmissionValue>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FieldName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FieldValue).HasMaxLength(2000);
        });
    }
}
