using SimpleInjector;

namespace AspNetCore.ApplicationBlocks.DependencyInjection
{
    /// <summary>
    /// A unit or grouping of service registrations for a given
    /// <see cref="Container" />.
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// Register services configured by this <see cref="IModule" />
        /// in the specified <see cref="Container" />.
        /// </summary>
        /// <param name="container">
        /// The <see cref="Container" /> in which this <c>IModule's</c>
        /// dependencies will be registered.
        /// </param>
        void RegisterServices(Container container);
    }
}
