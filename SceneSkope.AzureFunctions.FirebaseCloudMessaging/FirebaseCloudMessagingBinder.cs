using Matrix.Xmpp.Client;
using Microsoft.Azure.WebJobs.Host.Bindings;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SceneSkope.AzureFunctions.FirebaseCloudMessaging
{
    internal sealed class FirebaseCloudMessagingBinder : IValueProvider
    {
        private readonly ParameterInfo _parameter;
        public FirebaseCloudMessage Message { get; }

        public Type Type => _parameter.ParameterType;

        public FirebaseCloudMessagingBinder(ParameterInfo parameter, FirebaseCloudMessage message)
        {
            _parameter = parameter;
            Message = message;
        }

        public Task<object> GetValueAsync() => Task.FromResult<object>(Message);

        public string ToInvokeString() => "Test";
    }
}
