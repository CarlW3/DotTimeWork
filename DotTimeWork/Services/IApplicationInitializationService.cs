namespace DotTimeWork.Services
{
    public interface IApplicationInitializationService
    {
        /// <summary>
        /// Initializes the application settings including the time tracking folder path.
        /// This should be called once at application startup.
        /// </summary>
        void Initialize();
    }
}
