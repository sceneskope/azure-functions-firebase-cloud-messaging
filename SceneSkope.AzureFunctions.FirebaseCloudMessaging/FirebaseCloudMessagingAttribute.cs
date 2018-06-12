using Microsoft.Azure.WebJobs.Description;
using System;

namespace SceneSkope.AzureFunctions.FirebaseCloudMessaging
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public sealed class FirebaseCloudMessagingAttribute : Attribute
    {
        [AppSetting]
        public string Connection { get; set; }
    }
}
