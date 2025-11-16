using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace AetherhubEloFunctions.Commands;

public static class ContainerConfiguration
{
    public static IServiceCollection ConfigureCommands(this IServiceCollection services)
    {
        var commandProcessorType = typeof(CommandProcessor);
        var assembly = Assembly.GetExecutingAssembly();

        var processors = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && commandProcessorType.IsAssignableFrom(t))
            .ToList();

        foreach (var processorType in processors)
        {
            services.AddSingleton(processorType);
        }

        return services;
    }
}