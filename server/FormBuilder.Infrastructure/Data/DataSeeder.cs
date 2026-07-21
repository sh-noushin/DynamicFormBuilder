using FormBuilder.Core.Constants;
using FormBuilder.Core.Options;
using FormBuilder.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task EnsureDatabaseAsync(FormBuilderDbContext context)
    {
        if (!context.Database.IsInMemory())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }
    }

    public static async Task SeedAsync(
        FormBuilderDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        SeedOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.AdminPassword))
            throw new InvalidOperationException($"'{SeedOptions.SectionName}:{nameof(SeedOptions.AdminPassword)}' is required to seed the admin user.");
        if (string.IsNullOrWhiteSpace(options.UserPassword))
            throw new InvalidOperationException($"'{SeedOptions.SectionName}:{nameof(SeedOptions.UserPassword)}' is required to seed the sample user.");
        if (string.IsNullOrWhiteSpace(options.SuperAdminPassword))
            throw new InvalidOperationException($"'{SeedOptions.SectionName}:{nameof(SeedOptions.SuperAdminPassword)}' is required to seed the super admin.");

        await EnsureDatabaseAsync(context);
        var defaultOrg = await SeedDefaultOrganizationAsync(context);
        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager, options, defaultOrg.Id);
        await SeedSuperAdminAsync(userManager, options);

        if (await context.Forms.AnyAsync())
        {
            return;
        }

        await SeedFormsAsync(context, defaultOrg.Id);
        await SeedSampleSubmissionsAsync(context);
    }

    // Overload for tests that don't need Identity seeding
    public static async Task SeedAsync(FormBuilderDbContext context)
    {
        await EnsureDatabaseAsync(context);
        var defaultOrg = await SeedDefaultOrganizationAsync(context);

        if (await context.Forms.AnyAsync())
        {
            return;
        }

        await SeedFormsAsync(context, defaultOrg.Id);
        await SeedSampleSubmissionsAsync(context);
    }

    // Every DB — dev, test, seeded prod — gets one "Default Workspace"
    // org so users and forms have a tenant to belong to from the start.
    // Once real customers sign up they'll get their own org via the
    // /register endpoint; this default is only for the seeded admin/user.
    private static async Task<Organization> SeedDefaultOrganizationAsync(FormBuilderDbContext context)
    {
        var existing = await context.Organizations.FirstOrDefaultAsync(o => o.Slug == "default");
        if (existing != null) return existing;

        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Default Workspace",
            Slug = "default",
            CreatedAt = DateTime.UtcNow,
        };
        context.Organizations.Add(org);
        await context.SaveChangesAsync();
        return org;
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

        if (!await roleManager.RoleExistsAsync(Roles.SuperAdmin))
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.SuperAdmin));
        }
    }

    // Super admin owns no workspace, so OrganizationId is left at
    // Guid.Empty. Their role check is what gates the /superadmin API;
    // tenant-scoped queries never return anything for them (their org
    // doesn't match any Form/ApiKey row) which is the intended safe
    // default — they use dedicated endpoints instead.
    private static async Task SeedSuperAdminAsync(UserManager<User> userManager, SeedOptions options)
    {
        if (await userManager.FindByNameAsync("super") != null) return;

        var user = new User
        {
            UserName = "super",
            Email = "super@formbuilder.com",
            OrganizationId = Guid.Empty,
        };

        var result = await userManager.CreateAsync(user, options.SuperAdminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, Roles.SuperAdmin);
        }
    }

    private static async Task SeedUsersAsync(UserManager<User> userManager, SeedOptions options, Guid defaultOrgId)
    {
        if (await userManager.FindByNameAsync("admin") == null)
        {
            var adminUser = new User
            {
                UserName = "admin",
                Email = "admin@formbuilder.com",
                OrganizationId = defaultOrgId,
            };

            var result = await userManager.CreateAsync(adminUser, options.AdminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            }
        }

        if (await userManager.FindByNameAsync("user") == null)
        {
            var regularUser = new User
            {
                UserName = "user",
                Email = "user@formbuilder.com",
                OrganizationId = defaultOrgId,
            };

            var result = await userManager.CreateAsync(regularUser, options.UserPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(regularUser, Roles.User);
            }
        }
    }

    private static async Task SeedFormsAsync(FormBuilderDbContext context, Guid orgId)
    {
        var contactForm = new Form
        {
            OrganizationId = orgId,
            Name = "Contact Form",
            Description = "A comprehensive contact form for customer inquiries",
            Slug = FormBuilder.Core.Common.SlugGenerator.Generate(),
            BrandColor = "#6366f1",
            ThankYouMessage = "Thanks for reaching out — we usually respond within one business day.",
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
            OrganizationId = orgId,
            Name = "Customer Satisfaction Survey",
            Description = "Help us improve our services with your feedback",
            Slug = FormBuilder.Core.Common.SlugGenerator.Generate(),
            BrandColor = "#10b981",
            ThankYouMessage = "Thanks for the honest feedback — it directly shapes what we build next.",
            OneResponsePerEmail = true,
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
            OrganizationId = orgId,
            Name = "Event Registration",
            Description = "Register for our upcoming events",
            Slug = FormBuilder.Core.Common.SlugGenerator.Generate(),
            BrandColor = "#f59e0b",
            SendConfirmationEmail = true,
            ConfirmationEmailBody = "You're on the list. We'll email location + agenda details closer to the date.",
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

        // Showcase form: NPS survey — Rating field + follow-up Textarea +
        // one-response-per-email + a distinct brand color so the forms list
        // pops on first login.
        var npsForm = new Form
        {
            OrganizationId = orgId,
            Name = "NPS Survey",
            Description = "One-question survey with a follow-up",
            Slug = FormBuilder.Core.Common.SlugGenerator.Generate(),
            BrandColor = "#0ea5e9",
            ThankYouMessage = "Thanks — every score helps us understand where to focus.",
            OneResponsePerEmail = true,
            CreatedAt = DateTime.UtcNow.AddDays(-14),
            UpdatedAt = DateTime.UtcNow.AddDays(-14),
            IsActive = true,
            Versions = new List<FormVersion>
            {
                new()
                {
                    VersionNumber = 1,
                    Description = "Initial NPS survey",
                    CreatedAt = DateTime.UtcNow.AddDays(-14),
                    UpdatedAt = DateTime.UtcNow.AddDays(-14),
                    IsPublished = true,
                    IsCurrentVersion = true,
                    Fields = new List<FormVersionField>
                    {
                        new()
                        {
                            Name = "score",
                            Label = "How likely are you to recommend us?",
                            Type = FieldType.Rating,
                            IsRequired = true,
                            Order = 1,
                            HelpText = "Tap a star from 1 (unlikely) to 5 (very likely)",
                            IsVisible = true,
                        },
                        new()
                        {
                            Name = "reason",
                            Label = "What's the main reason for your score?",
                            Type = FieldType.Textarea,
                            IsRequired = false,
                            Order = 2,
                            Placeholder = "Optional but really useful",
                            IsVisible = true,
                        },
                    },
                },
            },
        };

        // Showcase form: Job application — multi-page (PageBreak),
        // File upload with per-field size + extension cap, Signature,
        // Hidden field to capture ?utm_source= for campaign attribution.
        var jobForm = new Form
        {
            OrganizationId = orgId,
            Name = "Job Application",
            Description = "Tell us about yourself",
            Slug = FormBuilder.Core.Common.SlugGenerator.Generate(),
            BrandColor = "#8b5cf6",
            SendConfirmationEmail = true,
            ConfirmationEmailBody = "Application received. Our team reviews within 5 business days.",
            CreatedAt = DateTime.UtcNow.AddDays(-7),
            UpdatedAt = DateTime.UtcNow.AddDays(-7),
            IsActive = true,
            Versions = new List<FormVersion>
            {
                new()
                {
                    VersionNumber = 1,
                    Description = "Initial application form",
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    UpdatedAt = DateTime.UtcNow.AddDays(-7),
                    IsPublished = true,
                    IsCurrentVersion = true,
                    Fields = new List<FormVersionField>
                    {
                        // Hidden field for campaign attribution via URL params.
                        new() { Name = "utm_source", Label = "Traffic source", Type = FieldType.HiddenField, Order = 1, IsVisible = true },

                        // Page 1: identity
                        new() { Name = "fullName", Label = "Full name", Type = FieldType.Text, IsRequired = true, Order = 2, IsVisible = true },
                        new() { Name = "email", Label = "Email", Type = FieldType.Email, IsRequired = true, Order = 3, IsVisible = true },
                        new() { Name = "phone", Label = "Phone (optional)", Type = FieldType.Phone, Order = 4, IsVisible = true },

                        // Page break separates identity from experience.
                        new() { Name = "pageBreak1", Label = "Experience", Type = FieldType.PageBreak, Order = 5, IsVisible = true },

                        new() { Name = "role", Label = "Role you're applying for", Type = FieldType.Select, IsRequired = true, Order = 6, IsVisible = true,
                                Options = "[{\"value\":\"eng\",\"label\":\"Software engineer\"},{\"value\":\"design\",\"label\":\"Designer\"},{\"value\":\"pm\",\"label\":\"Product manager\"}]" },
                        new() { Name = "yearsExperience", Label = "Years of experience", Type = FieldType.Number, IsRequired = true, Order = 7, IsVisible = true,
                                Validation = "{\"minimum\":0,\"maximum\":50,\"rangeMessage\":\"Enter a value between 0 and 50\"}" },

                        // Page break separates experience from files/signature.
                        new() { Name = "pageBreak2", Label = "Materials", Type = FieldType.PageBreak, Order = 8, IsVisible = true },

                        // File field with per-field constraints: PDF/DOCX, 5MB.
                        new() { Name = "resume", Label = "Resume", Type = FieldType.File, IsRequired = true, Order = 9, IsVisible = true,
                                HelpText = "PDF or Word document, up to 5 MB",
                                Validation = "{\"maxFileSizeMb\":5,\"allowedFileExtensions\":[\"pdf\",\"docx\"]}" },
                        new() { Name = "coverLetter", Label = "Cover letter (optional)", Type = FieldType.Textarea, Order = 10, IsVisible = true,
                                Placeholder = "Tell us what excites you about this role..." },
                        new() { Name = "signature", Label = "Signature", Type = FieldType.Signature, IsRequired = true, Order = 11, IsVisible = true,
                                HelpText = "Draw your signature to confirm the information above is accurate" },
                    },
                },
            },
        };

        context.Forms.AddRange(contactForm, surveyForm, registrationForm, npsForm, jobForm);
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

        // NPS submissions spanning the last two weeks so the analytics daily
        // chart has something to render out of the box. Includes tags on a
        // couple rows so the tag chip + filter UI has data to show off.
        var npsVersion = await context.FormVersions
            .Include(v => v.Fields)
            .FirstOrDefaultAsync(v => v.Form.Name == "NPS Survey" && v.IsCurrentVersion);
        if (npsVersion != null)
        {
            var random = new Random(42);
            var names = new[] { "Alex Chen", "Priya Patel", "Marcus Reed", "Sofia López", "Jordan Kim", "Emma Brown", "Kenji Tanaka", "Rachel Green" };
            var reasons = new[]
            {
                "Fast onboarding and clear UI - would definitely recommend.",
                "Loved it overall. Wish there was more third-party integration.",
                "Solid product but pricing feels steep for a small team.",
                "Perfect for our workflow. Support team is responsive.",
                "It works but the mobile experience could be smoother.",
                null, null,
                "Been using it for months, no complaints.",
            };
            var npsSubs = new List<FormSubmission>();
            for (var i = 0; i < 8; i++)
            {
                var score = random.Next(3, 6);
                var name = names[i];
                var email = name.ToLower().Replace(" ", ".").Replace("é", "e").Replace("ñ", "n") + "@example.com";
                npsSubs.Add(new FormSubmission
                {
                    FormVersionId = npsVersion.Id,
                    SubmittedAt = DateTime.UtcNow.AddDays(-random.Next(1, 14)).AddHours(-random.Next(0, 23)),
                    SubmitterName = name,
                    SubmitterEmail = email,
                    SubmitterIpAddress = $"192.168.1.{100 + i}",
                    Tags = score == 5 ? "promoter,follow-up" : (score <= 3 ? "detractor,follow-up" : null),
                    AdminNotes = score <= 3 ? "Reach out to understand the score." : null,
                    Values = new List<FormSubmissionValue>
                    {
                        new() { FieldName = "score", FieldValue = score.ToString() },
                        new() { FieldName = "reason", FieldValue = reasons[i] },
                    },
                });
            }
            context.FormSubmissions.AddRange(npsSubs);
            await context.SaveChangesAsync();
        }
    }
}
