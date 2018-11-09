namespace AspNetCore.ApplicationBlocks.Configuration
{
    /// <summary>
    /// General configuration applicable to any AspNetCore application.
    /// </summary>
    public interface IApplicationConfiguration
    {
        /// <summary>Gets the Environment in which the application is running</summary>
        string Environment { get; }

        /// <summary>Gets the name of the Application</summary>
        string Application { get; }

        /// <summary>Gets the current version of the Application. Usually a Git revision</summary>
        string Version { get; }

        /// <summary>Gets the name of the host the application is running on</summary>
        string Hostname { get; }

        /// <summary>
        /// Returns <c>true</c> if <see cref="Environment" /> is a "Production-like" environment name.
        /// </summary>
        bool IsProduction();

        /// <summary>
        /// Returns <c>true</c> if <see cref="Environment" /> is NOT considered "Production-like".
        /// </summary>
        bool IsDevelopment();
    }
}