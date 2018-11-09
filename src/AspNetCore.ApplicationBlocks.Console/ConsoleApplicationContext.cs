using System;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace AspNetCore.ApplicationBlocks.Console
{
    /// <summary>
    /// Represents the disposable scope of a console application's
    /// main execution.
    ///
    /// Encapsulates the composition root of the application, and
    /// disposes it when execution is completed.
    /// </summary>
    /// <remarks>
    /// When the <c>ConsoleApplicationContext</c> is created, it initiates
    /// an <see cref="AsyncScopedLifestyle" /> that serves as the main execution
    /// lifestyle with which all <c>Scoped</c> services are resolved.
    ///
    /// The scope is also disposed when execution is completed.
    /// </remarks>
    public class ConsoleApplicationContext : AbstractDisposable
    {
        private bool _disposed;
        private IDisposable scope;

        /// <summary>
        /// The composition root into which all services are registered.
        /// </summary>
        /// <returns></returns>
        public Container Container { get; private set; }

        internal ConsoleApplicationContext(
            Container container
        )
        {
            Container = container;
            scope = AsyncScopedLifestyle.BeginScope(Container);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // free other managed objects that implement
                    // IDisposable only
                    scope.Dispose();
                    Container.Dispose();
                }

                // release any unmanaged objects
                // set object references to null
                scope = null;
                Container = null;

                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}