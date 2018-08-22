using System;
using System.IO;
using System.Threading.Tasks;
using org.healthwise.ops.nugetsync.Configuration;
using org.healthwise.ops.nugetsync.Providers;
using Newtonsoft.Json;
using Serilog;

namespace org.healthwise.ops.nugetsync
{
    // TODO: Retries on HTTP calls
    class Program
    {
        public static async Task Main(string[] args)
        {
            // Setup logging
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.LiterateConsole()
                .WriteTo.RollingFile("log-{Date}.txt", 
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Message}\r\n{Exception}", retainedFileCountLimit: 7)
                .CreateLogger();
            
            // Load configuration
            Log.Logger.Information("Loading configuration...");

            var configFile = "config.json";
            if (args.Length > 3)
            {
                configFile = args[2];
                if (!File.Exists(configFile))
                {
                    Log.Logger.Error($"File {configFile} Does Not Exist");
                    return;
                }
            }

            var configuration = JsonConvert.DeserializeObject<ReplicationConfiguration>(
                File.ReadAllText(configFile));
            Log.Logger.Information($"Loaded configuration from {configFile}.");
            
            // Start replication tasks
            Log.Logger.Information("Starting replication tasks...");
            foreach (var replicationPair in configuration.ReplicationPairs)
            {
                // Build replicator
                Replicator replicator = null;
                
                switch (replicationPair.Type.ToLowerInvariant())
                {
                    case "nuget":
                        replicator = new Replicator(
                            new ProGetPackageProvider(replicationPair.Source.Url, replicationPair.Source.Token, replicationPair.Source.Username, replicationPair.Source.Password), 
                            new MyGetPackageProvider(replicationPair.Destination.Url, replicationPair.Destination.Token, replicationPair.Destination.Username, replicationPair.Destination.Password));
                        break;
                    default:
                        Log.Logger.Error("Unknown type {type} for replication pair {description}.", replicationPair.Type, replicationPair.Description);
                        break;
                }

                // Run replicator
                if (replicator != null)
                {
                    Log.Logger.Information("Starting replication task for replication pair: {description}...", replicationPair.Description, replicationPair);
                    await replicator.PerformReplication();
                    Log.Logger.Information("Finished replication task for replication pair: {description}.", replicationPair.Description, replicationPair);
                }
            }
            Log.Logger.Information("Finished replication tasks.");
        }
    }
}