using FormBuilder.Core;

namespace FormBuilder.API.Extensions;

public static class AutoMapperExtensions
{
    public static IServiceCollection AddFormBuilderAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile).Assembly);
        return services;
    }
}
