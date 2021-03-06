using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Business
{
    public static class DependencyProfile
    {
        public static IServiceCollection AddBusinessDependencies(this IServiceCollection services)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            
            services
                .AddMediatR(executingAssembly)
                // .AddMediatR(typeof(Business.Validation.Requests))
                .AddAutoMapper(executingAssembly);

            return services;
        }
    }
}
