using Matrix.Xmpp.Client;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SceneSkope.AzureFunctions.FirebaseCloudMessaging
{
    internal sealed class FirebaseCloudMessagingTriggerBinding : ITriggerBinding
    {
        public static readonly XName GcmName = XName.Get("gcm", "google:mobile:data");
        private readonly ParameterInfo _parameter;
        private readonly FirebaseCloudMessagingConfiguration _configuration;
        private readonly string _connectionString;
        private readonly ILogger _logger;

        public FirebaseCloudMessagingTriggerBinding(ParameterInfo parameter,
            FirebaseCloudMessagingConfiguration configuration,
            string connectionString,
            ILogger logger)
        {
            _parameter = parameter;
            _configuration = configuration;
            _connectionString = connectionString;
            _logger = logger;
        }

        public Type TriggerValueType => typeof(FirebaseCloudMessage);

        public IReadOnlyDictionary<string, Type> BindingDataContract => null;

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            var message = value as FirebaseCloudMessage;
            var valueBinder = new FirebaseCloudMessagingBinder(_parameter, message);
            var triggerData = new TriggerData(valueBinder, null);
            return Task.FromResult<ITriggerData>(triggerData);
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            IListener listener = new FirebaseCloudMessagingListener(context.Executor, _configuration, _connectionString, _logger);
            return Task.FromResult(listener);
        }

        public ParameterDescriptor ToParameterDescriptor() =>
            new ParameterDescriptor
            {
                Name = _parameter.Name,
                DisplayHints = new ParameterDisplayHints
                {
                    Prompt = "Firebase Cloud Messaging",
                    Description = "Fired",
                    DefaultValue = "Test"
                }
            };
    }
}
