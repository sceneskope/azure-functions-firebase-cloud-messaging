using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SceneSkope.AzureFunctions.FirebaseCloudMessaging
{
    internal sealed class FirebaseCloudMessageTriggerAttributeBindingProvider : ITriggerBindingProvider
    {
        private readonly INameResolver _nameResolver;
        private readonly IConverterManager _converterManager;
        private readonly FirebaseCloudMessagingConfiguration _configuration;
        private readonly ILogger _logger;

        public FirebaseCloudMessageTriggerAttributeBindingProvider(INameResolver nameResolver, IConverterManager converterManager, FirebaseCloudMessagingConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _nameResolver = nameResolver;
            _converterManager = converterManager;
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("FirebaseCloudMessaging"));
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var parameter = context.Parameter;
            var attribute = parameter.GetCustomAttribute<FirebaseCloudMessagingTriggerAttribute>(inherit: false);
            if (attribute == null)
            {
                return Task.FromResult<ITriggerBinding>(null);
            }

            var connectionString = _nameResolver.Resolve(attribute.Connection);
            var binding = new FirebaseCloudMessagingTriggerBinding(parameter, _configuration, connectionString, _logger);
            return Task.FromResult<ITriggerBinding>(binding);
        }
    }
}
