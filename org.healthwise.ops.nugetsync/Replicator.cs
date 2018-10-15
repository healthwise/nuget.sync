using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using org.healthwise.ops.nugetsync.Providers;
using Serilog;

namespace org.healthwise.ops.nugetsync
{
    public class Replicator
    {
        private const int ConcurrentTasksLimit = 8;   
        
        private readonly IPackageProvider _sourceProvider;
        private readonly IPackageProvider _destinationProvider;

        public Replicator(IPackageProvider sourceProvider, IPackageProvider destinationProvider)
        {
            _sourceProvider = sourceProvider;
            _destinationProvider = destinationProvider;
        }
        
        public async Task PerformReplication()
        {
            try
            {

                Log.Logger.Verbose("Concurrent task limit used for replication: {limit}.", ConcurrentTasksLimit);
                var throttler = new SemaphoreSlim(ConcurrentTasksLimit);

                Log.Logger.Information("Fetching packages from providers...");
                var fetchTasks = new[]
                {
                _sourceProvider.GetPackages(since: DateTime.MinValue),
                _destinationProvider.GetPackages(since: DateTime.MinValue)
            };

                await Task.WhenAll(fetchTasks);
                Log.Logger.Information("Fetched packages from providers.");

                var sourcePackages = fetchTasks[0].Result;
                var destinationPackages = fetchTasks[1].Result;

                var packagesToMirror = sourcePackages.Except(destinationPackages, PackageDefinition.FullComparer).ToList();

                Log.Logger.Information("# of packages on source: {numberOfPackages}", sourcePackages.Count);
                Log.Logger.Information("# of packages on destination: {numberOfPackages}", destinationPackages.Count);
                Log.Logger.Information("# of packages to replicate from source to destination: {numberOfPackages}", packagesToMirror.Count);

                // 1. Mirror packages from source that are not in destination
                var mirrorTasks = new List<Task>();
                foreach (var packageDefinition in packagesToMirror.OrderBy(p => p.PackageIdentifier).ThenBy(p => p.PackageVersion))
                {
                    mirrorTasks.Add(Task.Factory.StartNew(async () =>
                    {
                        try
                        {
                            await throttler.WaitAsync();

                            Log.Logger.Verbose(
                                "Replicating {packageType} package {packageIdentifier}@{packageVersion} from source to destination...",
                                packageDefinition.PackageType, packageDefinition.PackageIdentifier, packageDefinition.PackageVersion);

                            using (var packageStream = await StreamUtilities.MakeSeekable(
                                await _sourceProvider.GetPackageStream(packageDefinition)))
                            {
                                await _destinationProvider.PushPackage(packageDefinition, packageStream);
                            }

                            Log.Logger.Information(
                                "Replicated {packageType} package {packageIdentifier}@{packageVersion} from source to destination.",
                                packageDefinition.PackageType, packageDefinition.PackageIdentifier,
                                packageDefinition.PackageVersion);
                        }
                        catch (HttpRequestException requestException)
                        {
                            if (requestException.Message.IndexOf("404", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                Log.Logger.Error(
                                    "Replicating {packageType} package {packageIdentifier}@{packageVersion} failed: source returned 404 status code.",
                                     packageDefinition.PackageType, packageDefinition.PackageIdentifier, packageDefinition.PackageVersion);
                            }
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }).Unwrap());
                }

                await Task.WhenAll(mirrorTasks);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Application: NuGet Sync. Hostname: {System.Environment.MachineName} - An Error Occurred: {ex.Message}");

            }
        }
    }
}