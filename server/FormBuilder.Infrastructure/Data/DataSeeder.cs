using FormBuilder.Core.Constants;
using FormBuilder.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(FormBuilderDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Ensure the database is migrated (only for real databases)
        if (!context.Database.IsInMemory())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            // For in-memory databases, ensure the database is created
            await context.Database.EnsureCreatedAsync();
        }

        // Seed roles first
        await SeedRolesAsync(roleManager);

        // Seed users
        await SeedUsersAsync(userManager);

        // Check if data already exists
        if (await context.Forms.AnyAsync())
        {
            return; // Data already seeded
        }

        await SeedFormsAsync(context);
        await SeedSampleSubmissionsAsync(context);
    }

    // Overload for tests that don't need Identity seeding
    public static async Task SeedAsync(FormBuilderDbContext context)
    {
        // Ensure the database is migrated (only for real databases)
        if (!context.Database.IsInMemory())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            // For in-memory databases, ensure the database is created
            await context.Database.EnsureCreatedAsync();
        }

        // Check if data already exists
        if (await context.Forms.AnyAsync())
        {
            return; // Data already seeded
        }

        await SeedFormsAsync(context);
        await SeedSampleSubmissionsAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        if (!await roleManager.RoleExistsAsync(Roles.Admin))
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
        }

        if (!await roleManager.RoleExistsAsync(Roles.User))
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.User));
        }
    }

    private static async Task SeedUsersAsync(UserManager<User> userManager)
    {
        // Create admin user
        if (await userManager.FindByNameAsync("admin") == null)
        {
            var adminUser = new User
            {
                UserName = "admin",
                Email = "admin@formbuilder.com"
            };

            var result = await userManager.CreateAsync(adminUser, "123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            }
        }

        // Create regular user
        if (await userManager.FindByNameAsync("user") == null)
        {
            var regularUser = new User
            {
                UserName = "user",
                Email = "user@formbuilder.com"
            };

            var result = await userManager.CreateAsync(regularUser, "123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(regularUser, Roles.User);
            }
        }
    }

    private static async Task SeedFormsAsync(FormBuilderDbContext context)
    {
        var contactForm = new Form
        {
            Name = "Contact Form",
            Description = "A comprehensive contact form for customer inquiries",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            Versions = new List<FormVersion>
        {
            new()
            {
                VersionNumber = 1,
                Description = "Initial version of the contact form",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsPublished = true,
                IsCurrentVersion = true,
                Fields = new List<FormVersionField>
                {
                    new()
                    {
                        Name = "firstName",
                        Label = "First Name",
                        Type = FieldType.Text,
                        IsRequired = true,
                        Order = 1,
                        Placeholder = "Enter your first name",
                        HelpText = "Please provide your legal first name",
                        IsVisible = true,
                        IsReadOnly = false
                    },
                    new()
                    {
                        Name = "lastName",
                        Label = "Last Name",
                        Type = FieldType.Text,
                        IsRequired = true,
                        Order = 2,
                        Placeholder = "Enter your last name",
                        HelpText = "Please provide your legal last name",
                        IsVisible = true,
                        IsReadOnly = false
                    },
                    new()
                    {
                        Name = "email",
                        Label = "Email Address",
                        Type = FieldType.Email,
                        IsRequired = true,
                        Order = 3,
                        Placeholder = "your.email@example.com",
                        HelpText = "We'll use this to respond to your inquiry",
                        Validation = "^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$",
                        IsVisible = true,
                        IsReadOnly = false
                    },
                    new()
                    {
                        Name = "phone",
                        Label = "Phone Number",
                        Type = FieldType.Phone,
                        IsRequired = false,
                        Order = 4,
                        Placeholder = "(555) 123-4567",
                        HelpText = "Optional - for urgent inquiries",
                        IsVisible = true,
                        IsReadOnly = false
                    },
                    new()
                    {
                        Name = "inquiryType",
                        Label = "Type of Inquiry",
                        Type = FieldType.Select,
                        IsRequired = true,
                        Order = 5,
                        Options = "[{\"value\":\"general\",\"label\":\"General Question\"},{\"value\":\"support\",\"label\":\"Technical Support\"},{\"value\":\"billing\",\"label\":\"Billing Question\"},{\"value\":\"feature\",\"label\":\"Feature Request\"}]",
                        HelpText = "Please select the category that best describes your inquiry",
                        IsVisible = true,
                        IsReadOnly = false
                    },
                    new()
                    {
                        Name = "urgent",
                        Label = "This is an urgent inquiry",
                        Type = FieldType.Checkbox,
                        IsRequired = false,
                        Order = 6,
                        HelpText = "Check this if you need a response within 24 hours",
                        IsVisible = true,
                        IsReadOnly = false
                    },
                    new()
                    {
                        Name = "message",
                        Label = "Message",
                        Type = FieldType.Textarea,
                        IsRequired = true,
                        Order = 7,
                        Placeholder = "Please describe your inquiry in detail...",
                        HelpText = "Provide as much detail as possible to help us assist you",
                        IsVisible = true,
                        IsReadOnly = false
                    }
                }
            }
        }
        };

        var surveyForm = new Form
        {
            Name = "Customer Satisfaction Survey",
            Description = "Help us improve our services with your feedback",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            Versions = new List<FormVersion>
        {
            new()
            {
                VersionNumber = 1,
                Description = "Customer satisfaction survey v1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsPublished = true,
                IsCurrentVersion = true,
                Fields = new List<FormVersionField>
                {
                    new()
                    {
                        Name = "customerName",
                        Label = "Your Name",
                        Type = FieldType.Text,
                        IsRequired = true,
                        Order = 1,
                        Placeholder = "Enter your full name",
                        IsVisible = true,
                        IsReadOnly = false
                    },
                    new()
                    {
                        Name = "satisfactionRating",
                        Label = "Overall Satisfaction",
                        Type = FieldType.Radio,
                        IsRequired = true,
                        Order = 2,
                        Options = "[{\"value\":\"5\",\"label\":\"Very Satisfied\"},{\"value\":\"4\",\"label\":\"Satisfied\"},{\"value\":\"3\",\"label\":\"Neutral\"},{\"value\":\"2\",\"label\":\"Dissatisfied\"},{\"value\":\"1\",\"label\":\"Very Dissatisfied\"}]",
                        HelpText = "Rate your overall satisfaction with our service",
                        IsVisible = true,
                        IsReadOnly = false
                    },
                    new()
                    {
                        Name = "serviceDate",
                        Label = "Service Date",
                        Type = FieldType.Date,
                        IsRequired = true,
                        Order = 3,
                        HelpText = "When did you use our service?",
                        IsVisible = true,
                        IsReadOnly = false
                    },
                    new()
                    {
                        Name = "improvements",
                        Label = "Suggestions for Improvement",
                        Type = FieldType.Textarea,
                        IsRequired = false,
                        Order = 4,
                        Placeholder = "How can we improve our service?",
                        IsVisible = true,
                        IsReadOnly = false
                    },
                    new()
                    {
                        Name = "recommendToFriend",
                        Label = "Would you recommend us to a friend?",
                        Type = FieldType.Radio,
                        IsRequired = true,
                        Order = 5,
                        Options = "[{\"value\":\"yes\",\"label\":\"Yes, definitely\"},{\"value\":\"maybe\",\"label\":\"Maybe\"},{\"value\":\"no\",\"label\":\"No, probably not\"}]",
                        IsVisible = true,
                        IsReadOnly = false
                    }
                }
            }
        }
        };

        // Add a form with multiple versions to demonstrate versioning
        var registrationForm = new Form
        {
            Name = "Event Registration",
            Description = "Register for our upcoming events",
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            Versions = new List<FormVersion>
        {
            // Version 1 (older version)
            new()
            {
                VersionNumber = 1,
                Description = "Basic event registration form",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-30),
                IsPublished = true,
                IsCurrentVersion = false,
                Fields = new List<FormVersionField>
                {
                    new()
                    {
                        Name = "attendeeName",
                        Label = "Attendee Name",
                        Type = FieldType.Text,
                        IsRequired = true,
                        Order = 1,
                        IsVisible = true,
                        IsReadOnly = false
                    },
                    new()
                    {
                        Name = "eventType",
                        Label = "Event Type",
                        Type = FieldType.Select,
                        IsRequired = true,
                        Order = 2,
                        Options = "[{\"value\":\"workshop\",\"label\":\"Workshop\"},{\"value\":\"seminar\",\"label\":\"Seminar\"}]",
                        IsVisible = true,
                        IsReadOnly = false
                    }
                }
            },
            // Version 2 (current version)
            new()
            {
                VersionNumber = 2,
                Description = "Enhanced event registration with dietary preferences",
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow,
                IsPublished = true,
                IsCurrentVersion = true,
                Fields = new List<FormVersionField>
                {
                    new()
                    {
                        Name = "attendeeName",
                        Label = "Attendee Full Name",
                        Type = FieldType.Text,
                        IsRequired = true,
                        Order = 1,
                        Placeholder = "Enter your full name",
                        HelpText = "Name as it should appear on your badge",
                        IsVisible = true,
                        IsReadOnly = false
                    },
                    new()
                    {
                        Name = "attendeeEmail",
                        Label = "Email Address",
                        Type = FieldType.Email,
                        IsRequired = true,
                        Order = 2,
                        Placeholder = "your.email@example.com",
                        IsVisible = true,
                        IsReadOnly = false
                    },
                    new()
                    {
                        Name = "eventType",
                        Label = "Event Type",
                        Type = FieldType.Select,
                        IsRequired = true,
                        Order = 3,
                        Options = "[{\"value\":\"workshop\",\"label\":\"Workshop\"},{\"value\":\"seminar\",\"label\":\"Seminar\"},{\"value\":\"conference\",\"label\":\"Conference\"}]",
                        HelpText = "Select the type of event you're registering for",
                        IsVisible = true,
                        IsReadOnly = false
                    },
                    new()
                    {
                        Name = "dietaryRestrictions",
                        Label = "Dietary Restrictions",
                        Type = FieldType.Textarea,
                        IsRequired = false,
                        Order = 4,
                        Placeholder = "Please list any dietary restrictions or allergies...",
                        HelpText = "Help us accommodate your dietary needs",
                        IsVisible = true,
                        IsReadOnly = false
                    },
                    new()
                    {
                        Name = "newsletter",
                        Label = "Subscribe to our newsletter",
                        Type = FieldType.Checkbox,
                        IsRequired = false,
                        Order = 5,
                        HelpText = "Stay updated with our latest events and news",
                        IsVisible = true,
                        IsReadOnly = false
                    }
                }
            }
        }
        };

        context.Forms.AddRange(contactForm, surveyForm, registrationForm);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSampleSubmissionsAsync(FormBuilderDbContext context)
    {
        // Get the contact form version for sample submissions
        var contactFormVersion = await context.FormVersions
            .Include(v => v.Fields)
            .FirstOrDefaultAsync(v => v.Form.Name == "Contact Form" && v.IsCurrentVersion);

        if (contactFormVersion != null)
        {
            var sampleSubmissions = new List<FormSubmission>
        {
            new()
            {
                FormVersionId = contactFormVersion.Id,
                SubmittedAt = DateTime.UtcNow.AddDays(-5),
                SubmitterName = "John Doe",
                SubmitterEmail = "john.doe@example.com",
                SubmitterIpAddress = "192.168.1.100",
                Values = new List<FormSubmissionValue>
                {
                    new() { FieldName = "firstName", FieldValue = "John" },
                    new() { FieldName = "lastName", FieldValue = "Doe" },
                    new() { FieldName = "email", FieldValue = "john.doe@example.com" },
                    new() { FieldName = "phone", FieldValue = "(555) 123-4567" },
                    new() { FieldName = "inquiryType", FieldValue = "support" },
                    new() { FieldName = "urgent", FieldValue = "true" },
                    new() { FieldName = "message", FieldValue = "I'm having trouble accessing my account. Could you please help me reset my password?" }
                }
            },
            new()
            {
                FormVersionId = contactFormVersion.Id,
                SubmittedAt = DateTime.UtcNow.AddDays(-3),
                SubmitterName = "Jane Smith",
                SubmitterEmail = "jane.smith@example.com",
                SubmitterIpAddress = "192.168.1.101",
                Values = new List<FormSubmissionValue>
                {
                    new() { FieldName = "firstName", FieldValue = "Jane" },
                    new() { FieldName = "lastName", FieldValue = "Smith" },
                    new() { FieldName = "email", FieldValue = "jane.smith@example.com" },
                    new() { FieldName = "inquiryType", FieldValue = "feature" },
                    new() { FieldName = "urgent", FieldValue = "false" },
                    new() { FieldName = "message", FieldValue = "I would love to see a dark mode option in your application. This would really improve the user experience during evening hours." }
                }
            }
        };

            context.FormSubmissions.AddRange(sampleSubmissions);
            await context.SaveChangesAsync();
        }
    }
}
