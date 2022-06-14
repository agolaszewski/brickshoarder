using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Core.Queries;
using BricksHoarder.Core.Specification;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BricksHoarder.Domain
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDomain(this IServiceCollection services)
        {
            var domainAssembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == "BricksHoarder.Domain");

            services.Scan(scan =>
                scan.FromAssemblies(domainAssembly)
                    .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)))
                    .AsImplementedInterfaces().WithTransientLifetime()
            );

            services.Scan(scan =>
                scan.FromAssemblies(domainAssembly)
                    .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
                    .AsImplementedInterfaces().WithScopedLifetime()
            );

            services.Scan(scan =>
                scan.FromAssemblies(domainAssembly)
                    .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                    .AsImplementedInterfaces().WithScopedLifetime()
            );

            services.Scan(scan =>
                scan.FromAssemblies(domainAssembly)
                    .AddClasses(classes => classes.AssignableTo(typeof(IEventHandler<>)))
                    .AsImplementedInterfaces().WithScopedLifetime()
            );

            services.Scan(scan =>
                scan.FromAssemblies(domainAssembly)
                    .AddClasses(classes => classes.AssignableTo(typeof(ISpecificationForCommand<,>)))
                    .AsImplementedInterfaces().WithScopedLifetime()
            );

            services.Scan(scan =>
                scan.FromAssemblies(domainAssembly)
                    .AddClasses(filter => filter.Where(implementation => typeof(ICommand).IsAssignableFrom(implementation) && typeof(IRequest).IsAssignableFrom(implementation)))
                    .AsSelf()
                    .WithScopedLifetime()
            );

            services.Scan(scan =>
                scan.FromAssemblies(domainAssembly)
                    .AddClasses(filter => filter.Where(implementation => typeof(IQuery).IsAssignableFrom(implementation) && typeof(IRequest).IsAssignableFrom(implementation)))
                    .AsSelf()
                    .WithScopedLifetime()
            );

            services.Scan(scan =>
                scan.FromAssemblies(domainAssembly)
                    .AddClasses(filter => filter.Where(implementation => typeof(IQuery).IsAssignableFrom(implementation) && typeof(IRequest).IsAssignableFrom(implementation)))
                    .AsSelf()
                    .WithScopedLifetime()
            );

            services.Scan(scan =>
                scan.FromAssemblies(domainAssembly)
                    .AddClasses(classes => classes.AssignableTo(typeof(IAggregateMap<>)))
                    .AsImplementedInterfaces().WithScopedLifetime()
            );
        }
    }
}