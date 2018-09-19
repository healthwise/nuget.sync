using System.IO;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using org.healthwise.ops.nugetsync.Configuration;
using org.healthwise.ops.nugetsync.Providers;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using System.Net;
using org.healthwise.ops.nugetsync.Sinks;

namespace org.healthwise.ops.nugetsync
{
    // TODO: Retries on HTTP calls
    class Program
    {
        public static Task<int> Main(string[] args) => CommandLineApplication.ExecuteAsync<Program>(args);

        [Option(Description = "Config File", ShortName = "c")]
        public string ConfigFile { get; }

        private async Task OnExecuteAsync()
        {
            // Force TLS 1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Load configuration
            var configFile = "config.json";
            if (!string.IsNullOrEmpty(ConfigFile))
            {
                if (!File.Exists(ConfigFile))
                {
                    Log.Logger.Error($"File {ConfigFile} Does Not Exist");
                    return;
                }

                configFile = ConfigFile;
            }
            var configuration = JsonConvert.DeserializeObject<ReplicationConfiguration>(
                File.ReadAllText(configFile));

            // Setup logging
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.ChatBotSink(functionKey: configuration.MessageSetting.FunctionKey, serviceURL: configuration.MessageSetting.ServiceURL, channelData: configuration.MessageSetting.ChannelData, environment: configuration.MessageSetting.Environment)
                .WriteTo.LiterateConsole(restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.RollingFile("log-{Date}.txt", 
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Message}\r\n{Exception}", retainedFileCountLimit: 7)
                .CreateLogger();

            Log.Logger.Information("Loading configuration...");
        
            Log.Logger.Information($"Loaded configuration from {configFile}.");
            
            // Start replication tasks
            Log.Logger.Information("Starting replication tasks...");
            INuGetProviderFactory providerFactory = new NuGetProviderFactory();
            foreach (var replicationPair in configuration.ReplicationPairs)
            {
                // Build replicator
                Replicator replicator = null;
                
                switch (replicationPair.Type.ToLowerInvariant())
                {
                    case "nuget":
                        replicator = new Replicator(
                            providerFactory.LoadProvider(replicationPair.Source.Provider,replicationPair.Source.Url, replicationPair.Source.Token, replicationPair.Source.Username, replicationPair.Source.Password),
                            providerFactory.LoadProvider(replicationPair.Destination.Provider,replicationPair.Destination.Url, replicationPair.Destination.Token, replicationPair.Destination.Username, replicationPair.Destination.Password));
                        break;
                    case "npm":
                        replicator = new Replicator(
                            new NpmPackageProvider(replicationPair.Source.Url, replicationPair.Source.Token, replicationPair.Source.Username, replicationPair.Source.Password),
                            new NpmPackageProvider(replicationPair.Destination.Url, replicationPair.Destination.Token, replicationPair.Destination.Username, replicationPair.Destination.Password));
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