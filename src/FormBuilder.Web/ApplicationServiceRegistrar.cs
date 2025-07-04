using FormBuilder.Application.Contract.FormControls;
using FormBuilder.Application.Contract.FormControlValues;
using FormBuilder.Application.Contract.Forms;
using FormBuilder.Application.Contract.FormSubmissions;
using FormBuilder.Application.Contract.FormSubmissionValues;
using FormBuilder.Application.Contract.FormVersions;
using FormBuilder.Application.FormControls;
using FormBuilder.Application.FormControlValues;
using FormBuilder.Application.Forms;
using FormBuilder.Application.FormSubmissions;
using FormBuilder.Application.FormSubmissionValues;
using FormBuilder.Application.FormVersions;
using FormBuilder.DataAccess;
using FormBuilder.DataAccess.FormControls;
using FormBuilder.DataAccess.FormControlValues;
using FormBuilder.DataAccess.Forms;
using FormBuilder.DataAccess.FormSubmissions;
using FormBuilder.DataAccess.FormSubmissionValues;
using FormBuilder.DataAccess.FormVersions;
using FormBuilder.Domain.FormControls;
using FormBuilder.Domain.FormControlValues;
using FormBuilder.Domain.Forms;
using FormBuilder.Domain.FormSubmissions;
using FormBuilder.Domain.FormSubmissionValues;
using FormBuilder.Domain.FormVersions;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Web;

public static class ApplicationServiceRegistrar
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<FormBuilderDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // AutoMapper
        services.AddAutoMapper(typeof(ApplicationServiceRegistrar).Assembly);
        services.AddAutoMapper(typeof(FormControlMappingProfile).Assembly);
        services.AddAutoMapper(typeof(FormControlValueMappoinProfile).Assembly);
        services.AddAutoMapper(typeof(FormMappingProfile).Assembly);
        services.AddAutoMapper(typeof(FormSubmissionMappingProfile).Assembly);
        services.AddAutoMapper(typeof(FormSubmissionValueMappingProfile).Assembly);
        services.AddAutoMapper(typeof(FormVersionMappingProfile).Assembly);


        // Services
        services.AddScoped<IFormControlService, FormControlService>();
        services.AddScoped<IFormControlValueService, FormControlValueService>();
        services.AddScoped<IFormSubmissionService, FormSubmissionService>();
        services.AddScoped<IFormSubmissionValueService, FormSubmissionValueService>();
        services.AddScoped<IFormService, FormService>();
        services.AddScoped<IFormVersionService, FormVersionService>();

        // Managers
        services.AddScoped<IFormControlManager, FormControlManager>();
        services.AddScoped<IFormControlValueManager, FormControlValueManager>();
        services.AddScoped<IFormSubmissionManager, FormSubmissionManager>();
        services.AddScoped<IFormSubmissionValueManager, FormSubmissionValueManager>();
        services.AddScoped<IFormManager, FormManager>();
        services.AddScoped<IFormVersionManager, FormVersionManager>();

        // Repositories
        services.AddScoped<IFormControlRepository, FormControlRepository>();
        services.AddScoped<IFormControlValueRepository, FormControlValueRepository>();
        services.AddScoped<IFormSubmissionRepository, FormSubmissionRepository>();
        services.AddScoped<IFormSubmissionValueRepository, FormSubmissionValueRepository>();
        services.AddScoped<IFormRepository, FormRepository>();
        services.AddScoped<IFormVersionRepository, FormVersionRepository>();

        return services;
    }
}