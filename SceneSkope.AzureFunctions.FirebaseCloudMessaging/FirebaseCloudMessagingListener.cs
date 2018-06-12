using DotNetty.Transport.Channels;
using Matrix;
using Matrix.Network.Resolver;
using Matrix.Xmpp.Client;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data.Common;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SceneSkope.AzureFunctions.FirebaseCloudMessaging
{
    public class FirebaseCloudMessagingListener : IListener
    {
        private readonly ITriggeredFunctionExecutor _executor;
        private readonly FirebaseCloudMessagingConfiguration _configuration;
        private readonly string _connectionString;
        private readonly ILogger _logger;
        private XmppClient _client;
        private static readonly XName GcmName = XName.Get("gcm", "google:mobile:data");
        private bool _started;
        private bool _draining;

        public FirebaseCloudMessagingListener(ITriggeredFunctionExecutor executor, FirebaseCloudMessagingConfiguration configuration, string connectionString, ILogger logger)
        {
            _executor = executor;
            _configuration = configuration;
            _connectionString = connectionString;
            _logger = logger;
        }

        public void Cancel() => StopAsync(default).Wait();

        public void Dispose()
        {
            _client?.Dispose();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var builder = new DbConnectionStringBuilder { ConnectionString = _connectionString };
            var username = (string)builder["username"];
            var password = (string)builder["password"];
            int port;
            if (builder.TryGetValue("port", out var portValue))
            {
                if (!int.TryParse((string)portValue, out port))
                {
                    throw new ArgumentException($"Failed to parse port from {portValue}");
                }
            }
            else
            {
                port = 5235;
            }
            var hasLogging = builder.TryGetValue("logging", out var logging);

            var pipelineInitializerAction = new Action<IChannelPipeline>(pipeline =>
            {
                if (hasLogging)
                {
                    pipeline.AddFirst(new LoggingHandler(_logger));
                }
            });

            _client = new XmppClient(pipelineInitializerAction)
            {
                XmppDomain = "gcm.googleapis.com",
                Username = username,
                Password = password,
                HostnameResolver = new StaticNameResolver("gcm-xmpp.googleapis.com", port: port, directTls: true)
            };
            _client.XmppXElementStreamObserver
                .Subscribe(el => _logger.LogInformation("Got element: {Element}", el));

            _client.XmppXElementStreamObserver
                .Where(el => el is Message)
                .SelectMany(e => HandleMessageAsync((Message)e))
                .Subscribe();
            _client.XmppSessionStateObserver
                .Subscribe(async ss =>
                {
                    _logger.LogInformation("Session state changed to: {State}", ss);
                    if (_started && !_draining && (ss == SessionState.Disconnected))
                    {
                        await _client.ConnectAsync(default);
                    }
                });
            await _client.ConnectAsync(cancellationToken);
            _started = true;
        }

        private async Task<Unit> HandleMessageAsync(Message msg)
        {
            var gcm = msg.Element(GcmName);
            var gcmJo = JObject.Parse(gcm.Value);
            if (gcmJo.TryGetValue("control_type", out var controlTypeToken)
                && (controlTypeToken.Type == JTokenType.String))
            {
                switch ((string)controlTypeToken)
                {
                    case "CONNECTION_DRAINING":
                        _logger.LogInformation("Starting to drain");
                        _draining = true;
                        break;

                    default:
                        _logger.LogInformation("Got a control type: {Json}", gcmJo);
                        break;
                }
            }
            else
            {
                var from = (string)gcmJo["from"];
                var category = (string)gcmJo["category"];
                var messageId = (string)gcmJo["message_id"];
                var data = (JObject)gcmJo["data"];
                var message = new FirebaseCloudMessage(from, category, messageId, data);
                var triggerData = new TriggeredFunctionData { TriggerValue = message };

                var result = await _executor.TryExecuteAsync(triggerData, default);
                if (result.Succeeded)
                {
                    try
                    {
                        await SendAckFor(message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending ack: {Exception}", ex.Message);
                    }
                }
                else
                {
                    _logger.LogError(result.Exception, "Error handling message: {Exception}", result.Exception.Message);
                }
            }
            return Unit.Default;
        }

        private async Task SendAckFor(FirebaseCloudMessage message)
        {
            var ackObj = new { to = message.From, message_id = message.MessageId, message_type = "ack" };
            var ack = new Message();
            ack.Add(new XElement(GcmName, JsonConvert.SerializeObject(ackObj, Formatting.None)));
            await _client.SendAsync(ack);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping...");
            _started = false;
            var client = Interlocked.Exchange(ref _client, default);
            if (client != null)
            {
                await client.DisconnectAsync();
            }
        }
    }
}
