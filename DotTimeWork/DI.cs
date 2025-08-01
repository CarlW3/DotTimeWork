using DotTimeWork.Commands;
using DotTimeWork.ConsoleService;
using DotTimeWork.DataProvider;
using DotTimeWork.Developer;
using DotTimeWork.Project;
using DotTimeWork.Services;
using DotTimeWork.TimeTracker;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace DotTimeWork
{
    internal static class DI
    {
        private static IServiceProvider _serviceProvider;


        public static int CountOfDependencies { get; private set; }

        public static void InitDependencyInjection()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            CountOfDependencies = serviceCollection.Count;
            _serviceProvider = serviceCollection.BuildServiceProvider();
            
            // Initialize the application (including time tracking folder setup)
            var initializationService = GetService<IApplicationInitializationService>();
            initializationService.Initialize();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IInputAndOutputService, InputAndOutputService>();
            services.AddSingleton<IProjectConfigDataProvider, ProjectConfigDataJson>();
            services.AddSingleton<ITaskTimeTrackerDataProvider,TaskTimeTrackerDataJson>();
            services.AddSingleton<IProjectConfigController, ProjectConfigController>();
            services.AddSingleton<IDeveloperConfigController, DeveloperConfigController>();
            services.AddSingleton<IApplicationInitializationService, ApplicationInitializationService>();
            services.AddTransient<ITaskTimeTracker, TaskTimeTracker>();
            services.AddTransient<ITotalWorkingTimeCalculator, TotalWorkingTimeCalculator>();
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

        public static T GetService<T>()
        {
            return _serviceProvider.GetRequiredService<T>();
        }

        public static List<Command> GetAllCommands()
        {
            return _serviceProvider.GetServices<Command>().ToList();
        }
    }
}
