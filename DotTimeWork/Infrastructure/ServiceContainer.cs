using DotTimeWork.Commands;
using DotTimeWork.ConsoleService;
using DotTimeWork.DataProvider;
using DotTimeWork.Developer;
using DotTimeWork.Project;
using DotTimeWork.Services;
using DotTimeWork.TimeTracker;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace DotTimeWork.Infrastructure
{
    /// <summary>
    /// Centralized service container for dependency injection
    /// </summary>
    internal static class ServiceContainer
    {
        private static IServiceProvider? _serviceProvider;
        private static readonly object _lock = new object();

        public static IServiceProvider ServiceProvider
        {
            get
            {
                if (_serviceProvider == null)
                {
                    throw new InvalidOperationException("Service container has not been initialized. Call Initialize() first.");
                }
                return _serviceProvider;
            }
        }

        public static bool IsInitialized => _serviceProvider != null;

        public static void Initialize()
        {
            lock (_lock)
            {
                if (_serviceProvider != null)
                {
                    return; // Already initialized
                }

                var services = new ServiceCollection();
                ConfigureServices(services);
                _serviceProvider = services.BuildServiceProvider();

                // Initialize the application
                var initService = GetService<IApplicationInitializationService>();
                initService.Initialize();
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Core Services
            services.AddSingleton<IInputAndOutputService, InputAndOutputService>();
            
            // Data Providers
            services.AddSingleton<IProjectConfigDataProvider, ProjectConfigDataJson>();
            services.AddSingleton<ITaskTimeTrackerDataProvider, TaskTimeTrackerDataJson>();
            
            // Controllers
            services.AddSingleton<IProjectConfigController, ProjectConfigController>();
            services.AddSingleton<IDeveloperConfigController, DeveloperConfigController>();
            
            // Services
            services.AddSingleton<IApplicationInitializationService, ApplicationInitializationService>();
            services.AddTransient<ITaskTimeTracker, TaskTimeTracker>();
            services.AddTransient<ITotalWorkingTimeCalculator, TotalWorkingTimeCalculator>();
            
            // Commands
            RegisterCommands(services);
        }

        private static void RegisterCommands(IServiceCollection services)
        {
            services.AddTransient<Command, CreateProjectCommand>();
            services.AddTransient<Command, DetailsTaskCommand>();
            services.AddTransient<Command, DeveloperCommand>();
            services.AddTransient<Command, EndTaskCommand>();
            services.AddTransient<Command, InfoCommand>();
            services.AddTransient<Command, ListTaskCommand>();
            services.AddTransient<Command, StartTaskCommand>();
            services.AddTransient<Command, WorkCommand>();
            services.AddTransient<Command, CommentCommand>();
            services.AddTransient<Command, ReportCommand>();
        }

        public static T GetService<T>() where T : notnull
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        public static IEnumerable<Command> GetAllCommands()
        {
            return ServiceProvider.GetServices<Command>();
        }

        public static void Dispose()
        {
            lock (_lock)
            {
                if (_serviceProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                _serviceProvider = null;
            }
        }
    }
}
