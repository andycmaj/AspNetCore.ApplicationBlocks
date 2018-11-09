using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SerilogEventLogger;
using Microsoft.DotNet.PlatformAbstractions;

namespace AspNetCore.ApplicationBlocks.Services
{
    /// <summary>
    /// A service that monitors Docker container memory utilizations and tries to proactively run garbage collection if
    /// the memory limits get too close to the set threshold.
    /// </summary>
    public sealed class GarbageTruck : IDisposable
    {
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly CancellationToken cancellationToken;
        private readonly IEventLogger<GarbageTruck> garbageLogger;

        /// <summary>
        /// Gets or set a value between 0 and 1 that indicates the collection threshold of this service, as a percentage.
        /// </summary>
        public float CollectionThreshold { get; set; } = 0.9f;

        /// <summary>
        /// Gets or sets the time to sleep between memory checks.
        /// </summary>
        public TimeSpan PollingPeriod { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Gets or sets a value that enables metric logging if true.
        /// </summary>
        public bool EnableMetricsLogging { get; set; } = false;

        /// <summary>
        /// Constructs a new instance of the GarbageTruck.
        /// </summary>
        /// <param name="garbageLogger">Event logger for metrics and events.</param>
        /// <param name="cancellationToken">A cancellation token to stop the garbage truck execution.</param>
        public GarbageTruck(IEventLogger<GarbageTruck> garbageLogger, CancellationToken cancellationToken)
            : this(cancellationToken)
        {
            this.garbageLogger = garbageLogger;
        }

        /// <summary>
        /// Constructs a new instance of the GarbageTruck.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to stop the garbage truck execution.</param>
        public GarbageTruck(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Constructs a new instance of the GarbageTruck.
        /// </summary>
        /// <param name="garbageLogger">Event logger for metrics and events.</param>
        public GarbageTruck(IEventLogger<GarbageTruck> garbageLogger)
            : this()
        {
            this.garbageLogger = garbageLogger;
        }

        /// <summary>
        /// Constructs a new instance of the GarbageTruck.
        /// </summary>
        public GarbageTruck()
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            this.cancellationToken = this.cancellationTokenSource.Token;
        }

        void IDisposable.Dispose()
        {
            this.cancellationTokenSource.Dispose();
        }

        /// <summary>
        /// Stops the service execution.
        /// </summary>
        public void Cancel()
        {
            if (cancellationTokenSource == null)
            {
                throw new NotSupportedException($"Cannot call Cancel when initialized with a {nameof(CancellationToken)}.");
            }

            this.cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Starts the service.
        /// </summary>
        /// <returns>A task to indicate when the run has completed.</returns>
        public async Task StartAsync()
        {
            if (RuntimeEnvironment.OperatingSystemPlatform != Platform.Linux)
            {
                LogMessage($"{nameof(GarbageTruck)}NotSupported", $"{nameof(GarbageTruck)} does not support this platform.");
                return;
            }

#if NETSTANDARD2_0
            var memoryLimit = GetMemoryLimit();
#else
            var memoryLimit = await GetMemoryLimitAsync(this.cancellationToken);
#endif

            LogMessage($"{nameof(GarbageTruck)}MemoryLimitDetected", $"{nameof(GarbageTruck)} Memory Limit", new
            {
                MemoryLimit = memoryLimit
            });

            while (!this.cancellationToken.IsCancellationRequested)
            {
#if NETSTANDARD2_0
                long totalMemory = GetMemoryUsage();
#else
                long totalMemory = await GetMemoryUsageAsync(this.cancellationToken);
#endif

                if (EnableMetricsLogging)
                {
                    garbageLogger?.Count("GCTotalMemory", (int)totalMemory);
                }

                if (totalMemory > (long)(memoryLimit * CollectionThreshold))
                {
                    if (EnableMetricsLogging)
                    {
                        garbageLogger?.MoveGauge("GCCollect");
                    }

                    GC.Collect();
                }

                await Task.Delay(this.PollingPeriod);
            }
        }

#if NETSTANDARD2_0
        private static long GetMemoryLimit()
            => GetCgroupMemoryData("memory.limit_in_bytes");

        private static long GetMemoryUsage()
            => GetCgroupMemoryData("memory.usage_in_bytes");

        private static long GetCgroupMemoryData(string fileName)
        {
            if (!long.TryParse(File.ReadAllText(Path.Combine("/sys/fs/cgroup/memory/", fileName)), out long contents))
            {
                throw new InvalidOperationException("Could not parse contents of " + fileName);
            }

            return contents;
        }
#else
        private static Task<long> GetMemoryLimitAsync(CancellationToken cancellationToken)
            => GetCgroupMemoryDataAsync("memory.limit_in_bytes", cancellationToken);

        private static Task<long> GetMemoryUsageAsync(CancellationToken cancellationToken)
            => GetCgroupMemoryDataAsync("memory.usage_in_bytes", cancellationToken);

        private static async Task<long> GetCgroupMemoryDataAsync(string fileName, CancellationToken cancellationToken)
        {
            if (!long.TryParse(await File.ReadAllTextAsync(Path.Combine("/sys/fs/cgroup/memory/", fileName), cancellationToken), out long contents))
            {
                throw new InvalidOperationException("Could not parse contents of " + fileName);
            }

            return contents;
        }
#endif

        private void LogMessage(string eventName, string message, object data = null)
        {
            if (this.garbageLogger == null)
            {
                Console.WriteLine(message);
                Console.WriteLine(data);
            }
            else
            {
                this.garbageLogger.LogEvent(eventName, Severity.Information, data);
            }
        }
    }
}