using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;
using System;

namespace SceneSkope.AzureFunctions.FirebaseCloudMessaging
{
    public class FirebaseCloudMessagingConfiguration : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            context.ApplyConfig(this, "firebaseCloudMessaging");

            var nameResolver = context.Config.GetService<INameResolver>();
            var extensions = context.Config.GetService<IExtensionRegistry>();
            var converterManager = context.Config.GetService<IConverterManager>();

            var triggerBindingProvider = new FirebaseCloudMessageTriggerAttributeBindingProvider(
                nameResolver,
                converterManager,
                this,
                context.Config.LoggerFactory
            );
            extensions.RegisterExtension<ITriggerBindingProvider>(triggerBindingProvider);

            context.AddBindingRule<FirebaseCloudMessagingTriggerAttribute>()
                .BindToTrigger(triggerBindingProvider);
        }
    }
}
