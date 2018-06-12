using Microsoft.Azure.WebJobs.Description;
using System;

namespace SceneSkope.AzureFunctions.FirebaseCloudMessaging
{
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public sealed class FirebaseCloudMessagingTriggerAttribute : Attribute
    {
        [AppSetting]
        public string Connection { get; set; }
    }
}
