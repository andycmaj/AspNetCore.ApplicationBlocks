namespace AspNetCore.ApplicationBlocks.Bootstrapping
{
    /// <summary>
    /// A custom application-bootstrapping action to be executed
    /// at application startup.
    /// </summary>
    /// <remarks>
    /// Typically used to perform one-time actions like running database
    /// migrations, ensuring an S3 bucket exists, etc.
    /// </remarks>
    public interface IBootstrapper
    {
        /// <summary>
        /// Perform bootstrapping actions specific to this <c>IBootstrapper</c>.
        /// Called by the application host immediately after initializing
        /// all application dependencies in the composition root.
        /// </summary>
        void Bootstrap();
    }
}