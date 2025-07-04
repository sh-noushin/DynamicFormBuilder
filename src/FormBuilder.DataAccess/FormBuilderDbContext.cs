using FormBuilder.Domain.FormControls;
using FormBuilder.Domain.FormControlValues;
using FormBuilder.Domain.Forms;
using FormBuilder.Domain.FormSubmissions;
using FormBuilder.Domain.FormSubmissionValues;
using FormBuilder.Domain.FormVersions;
using FormBuilder.Domain.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.DataAccess;

public class FormBuilderDbContext : DbContext
{
    public DbSet<Form> Forms { get; set; }
    public DbSet<FormVersion> FormVersions { get; set; }
    public DbSet<FormControl> FormControls { get; set; }
    public DbSet<FormControlValue> FormControlValues { get; set; }
    public DbSet<FormSubmission> FormSubmissions { get; set; }
    public DbSet<FormSubmissionValue> FormSubmissionValues { get; set; }

    public FormBuilderDbContext(DbContextOptions<FormBuilderDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Form>()
            .HasMany(f => f.Versions)
            .WithOne(v => v.Form)
            .HasForeignKey(v => v.FormId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FormVersion>()
            .HasMany(v => v.Controls)
            .WithOne(c => c.FormVersion)
            .HasForeignKey(c => c.FormVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FormControl>()
            .HasMany(c => c.Values)
            .WithOne(o => o.FormControl)
            .HasForeignKey(o => o.FormControlId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FormControl>()
            .HasMany(c => c.SubmissionValues)
            .WithOne(v => v.FormControl)
            .HasForeignKey(v => v.FormControlId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FormSubmission>()
            .HasOne(s => s.FormVersion)
            .WithMany()
            .HasForeignKey(s => s.FormVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FormSubmissionValue>()
            .HasOne(v => v.FormSubmission)
            .WithMany(s => s.Values)
            .HasForeignKey(v => v.FormSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Inside OnModelCreating(ModelBuilder modelBuilder)

        modelBuilder.Entity<Form>().HasData(
     new Form { Id = Guid.Parse("332ce45d-814e-46d3-8e1a-a129ab63b58d"), Name = "Survey Form 1" },
     new Form { Id = Guid.Parse("2daa2bec-d937-44e1-8b3e-9c2331acc94b"), Name = "Survey Form 2" },
     new Form { Id = Guid.Parse("ef1ac28c-b2b4-4a1e-bc99-7ad17b0c5461"), Name = "Survey Form 3" }
 );

        modelBuilder.Entity<FormVersion>().HasData(
            new FormVersion { Id = Guid.Parse("d372874d-577c-49d9-ae98-cb445e6a7b9c"), FormId = Guid.Parse("332ce45d-814e-46d3-8e1a-a129ab63b58d"), Version = "v1.0", CreatedAt = DateTime.Parse("2025-07-01T08:00:00Z") },
            new FormVersion { Id = Guid.Parse("309f9fe8-1975-4526-821d-e13abb8ad0dc"), FormId = Guid.Parse("332ce45d-814e-46d3-8e1a-a129ab63b58d"), Version = "v2.0", CreatedAt = DateTime.Parse("2025-07-01T09:00:00Z") },
            new FormVersion { Id = Guid.Parse("cc3d790a-da55-47db-98b7-75dac4dd5de5"), FormId = Guid.Parse("2daa2bec-d937-44e1-8b3e-9c2331acc94b"), Version = "v1.0", CreatedAt = DateTime.Parse("2025-07-01T10:00:00Z") },
            new FormVersion { Id = Guid.Parse("a0727ace-3630-4c6c-8583-432df14cffc9"), FormId = Guid.Parse("2daa2bec-d937-44e1-8b3e-9c2331acc94b"), Version = "v2.0", CreatedAt = DateTime.Parse("2025-07-01T11:00:00Z") },
            new FormVersion { Id = Guid.Parse("f3b7fa94-1bef-4f8d-835d-73865f07c77f"), FormId = Guid.Parse("ef1ac28c-b2b4-4a1e-bc99-7ad17b0c5461"), Version = "v1.0", CreatedAt = DateTime.Parse("2025-07-01T12:00:00Z") },
            new FormVersion { Id = Guid.Parse("488cc030-a7a9-4135-81b1-cd4338fddf0c"), FormId = Guid.Parse("ef1ac28c-b2b4-4a1e-bc99-7ad17b0c5461"), Version = "v2.0", CreatedAt = DateTime.Parse("2025-07-01T13:00:00Z") }
        );

        modelBuilder.Entity<FormControl>().HasData(
            new FormControl { Id = Guid.Parse("aaa111aa-0000-0000-0000-000000000001"), Label = "Name", Type = ControlType.TextBox, IsRequired = true, FormVersionId = Guid.Parse("d372874d-577c-49d9-ae98-cb445e6a7b9c") },
            new FormControl { Id = Guid.Parse("aaa111aa-0000-0000-0000-000000000002"), Label = "Email", Type = ControlType.Email, IsRequired = true, FormVersionId = Guid.Parse("d372874d-577c-49d9-ae98-cb445e6a7b9c") },
            new FormControl { Id = Guid.Parse("aaa111aa-0000-0000-0000-000000000003"), Label = "Gender", Type = ControlType.RadioButton, IsRequired = false, FormVersionId = Guid.Parse("d372874d-577c-49d9-ae98-cb445e6a7b9c") }
        );

        modelBuilder.Entity<FormControlValue>().HasData(
            new FormControlValue { Id = Guid.Parse("ccc111aa-0000-0000-0000-000000000001"), FormControlId = Guid.Parse("aaa111aa-0000-0000-0000-000000000003"), Value = "Male" },
            new FormControlValue { Id = Guid.Parse("ccc111aa-0000-0000-0000-000000000002"), FormControlId = Guid.Parse("aaa111aa-0000-0000-0000-000000000003"), Value = "Female" }
        );

        modelBuilder.Entity<FormSubmission>().HasData(
            new FormSubmission { Id = Guid.Parse("ddd111aa-0000-0000-0000-000000000001"), FormVersionId = Guid.Parse("d372874d-577c-49d9-ae98-cb445e6a7b9c"), SubmittedAt = DateTime.Parse("2025-07-01T14:00:00Z") }
        );

        modelBuilder.Entity<FormSubmissionValue>().HasData(
            new FormSubmissionValue { Id = Guid.Parse("eee111aa-0000-0000-0000-000000000001"), FormSubmissionId = Guid.Parse("ddd111aa-0000-0000-0000-000000000001"), FormControlId = Guid.Parse("aaa111aa-0000-0000-0000-000000000001"), Value = "John Doe" },
            new FormSubmissionValue { Id = Guid.Parse("eee111aa-0000-0000-0000-000000000002"), FormSubmissionId = Guid.Parse("ddd111aa-0000-0000-0000-000000000001"), FormControlId = Guid.Parse("aaa111aa-0000-0000-0000-000000000002"), Value = "john@example.com" },
            new FormSubmissionValue { Id = Guid.Parse("eee111aa-0000-0000-0000-000000000003"), FormSubmissionId = Guid.Parse("ddd111aa-0000-0000-0000-000000000001"), FormControlId = Guid.Parse("aaa111aa-0000-0000-0000-000000000003"), Value = "Male" }
        );


    }
}
