using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using Serilog.Core;
using Serilog.Events;

namespace org.healthwise.ops.nugetsync
{
    public class ChatBotSink : ILogEventSink
    {
        private readonly string _functionKey;
        private readonly string _serviceURL;
        private readonly string _channelData;
        private readonly string _environment;
        private readonly IFormatProvider _formatProvider;

        public ChatBotSink(IFormatProvider formatProvider, string functionKey, string serviceURL, string channelData, string environment)
        {
            _formatProvider = formatProvider;
            _functionKey = functionKey;
            _serviceURL = serviceURL;
            _channelData = channelData;
            _environment = environment;
        }

        // Construct a sink that passes messages to the specificed chat account.
        public void Emit(LogEvent logEvent)
        {
            // Ensure only Error and Fatal level messages are sent to the sink
            if (logEvent.Level != LogEventLevel.Error && logEvent.Level != LogEventLevel.Fatal)
                return;

            JObject json = JObject.FromObject(new
            {
                type = "message",
                serviceURL = _serviceURL,
                channelData = new {teamsChannelId = _channelData},
                message = logEvent.RenderMessage(_formatProvider)
            });

            HttpContent content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                Uri baseUrl = new Uri($"https://hw-{_environment}-botservice-unclehab.azurewebsites.net");
                client.BaseAddress = baseUrl; //Base part of URL (like https://api.healthwise.org)

                string restOfUrl = $"/api/MessageProxy?code={_functionKey}";
                var result = client.PostAsync(restOfUrl, content).Result; //Rest of url - API endpoint - /my/endpoint

            }
        }
    }
}
