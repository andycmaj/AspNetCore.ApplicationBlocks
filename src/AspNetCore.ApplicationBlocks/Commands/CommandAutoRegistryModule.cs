using System;
using SimpleInjector;
using Microsoft.Extensions.DependencyModel;
using AspNetCore.ApplicationBlocks.DependencyInjection;

namespace AspNetCore.ApplicationBlocks.Commands
{
    /// <summary>
    /// Discovers and registers Commands and their Handlers, as well
    /// as the <see cref="ICommandRouter" />.
    /// </summary>
    public class CommandAutoRegistryModule : IModule
    {
        private readonly Lifestyle commandLifestyle;
        private readonly DependencyContext dependencyContext;

        public CommandAutoRegistryModule() : this(Lifestyle.Transient)
        {
        }

        public CommandAutoRegistryModule(
            Lifestyle commandLifestyle,
            DependencyContext dependencyContext = null
        )
        {
            this.commandLifestyle = commandLifestyle;
            this.dependencyContext = dependencyContext ?? DependencyContext.Default;
        }

        public void RegisterServices(Container container)
        {
            Action<CommandRegistrator.CommandHandlerRegistration> register =
                registration =>
                {
                    if (registration.IsDecorator)
                    {
                        // don't register decorators. let consumers do that.
                    }
                    else
                    {
                        container.Register(
                            registration.Interface,
                            registration.Implementation,
                            commandLifestyle);
                    }
                };

            var registrator = new CommandRegistrator(register, dependencyContext);

            registrator.RegisterCommands();

            container.RegisterSingleton<ICommandHandlerFactory, SimpleInjectorCommandHandlerFactory>();
            container.RegisterSingleton<ICommandRouter, CommandRouter>();

            container.RegisterDecorator(typeof(IActionHandler<>), typeof(LoggingActionHandlerDecorator<>));
            container.RegisterDecorator(typeof(IActionHandlerAsync<>), typeof(LoggingAsyncActionHandlerDecorator<>));
            container.RegisterDecorator(typeof(IFunctionHandler<,>), typeof(LoggingFunctionDecorator<,>));
            container.RegisterDecorator(typeof(IFunctionHandlerAsync<,>), typeof(LoggingAsyncFunctionDecorator<,>));
        }
    }
}
