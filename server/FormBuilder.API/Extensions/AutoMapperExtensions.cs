using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using FormBuilder.Core;

namespace FormBuilder.API.Extensions;

public static class AutoMapperExtensions
{
    public static IServiceCollection AddFormBuilderAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => { }, typeof(MappingProfile));
        return services;
    }
}
