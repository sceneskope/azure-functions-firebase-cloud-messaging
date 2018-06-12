using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading.Tasks;

namespace SceneSkope.AzureFunctions.FirebaseCloudMessaging
{
    internal class LoggingHandler : ChannelHandlerAdapter
    {
        private readonly ILogger logger;

        public LoggingHandler(ILogger logger)
        {
            this.logger = logger;
        }

        public LogLevel Level { get; } = LogLevel.Trace;

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            this.logger.LogInformation(this.Format(context, "RECV", message));
            context.FireChannelRead(message);
        }

        public override Task WriteAsync(IChannelHandlerContext context, object message)
        {
            this.logger.LogInformation(this.Format(context, "SEND", message));
            return context.WriteAsync(message);
        }

        protected virtual string Format(IChannelHandlerContext ctx, string eventName)
        {
            string chStr = ctx.Channel.ToString();
            return new StringBuilder(chStr.Length + 1 + eventName.Length)
                .Append(chStr)
                .Append(' ')
                .Append(eventName)
                .ToString();
        }

        protected virtual string Format(IChannelHandlerContext ctx, string eventName, object arg)
        {
            if (arg is IByteBuffer)
            {
                return this.FormatByteBuffer(ctx, eventName, (IByteBuffer)arg);
            }
            else
            {
                return this.FormatSimple(ctx, eventName, arg);
            }
        }

        private string FormatByteBuffer(IChannelHandlerContext ctx, string eventName, IByteBuffer msg)
        {
            string chStr = ctx.Channel.ToString();
            int length = msg.ReadableBytes;
            if (length == 0)
            {
                var buf = new StringBuilder(eventName.Length + 2);
                buf.Append(eventName).Append(": ");
                return buf.ToString();
            }
            else
            {
                var buf = new StringBuilder(eventName.Length + 2 + msg.ReadableBytes);

                byte[] resBuf = new byte[msg.ReadableBytes];
                msg.GetBytes(0, resBuf, 0, msg.ReadableBytes);
                var utf8String = Encoding.UTF8.GetString(resBuf);
                buf.Append(eventName).Append(": ").Append(utf8String);

                return buf.ToString();
            }
        }

        private string FormatSimple(IChannelHandlerContext ctx, string eventName, object msg)
        {
            string chStr = ctx.Channel.ToString();
            string msgStr = msg.ToString();
            var buf = new StringBuilder(chStr.Length + 1 + eventName.Length + 2 + msgStr.Length);
            return buf.Append(chStr).Append(' ').Append(eventName).Append(": ").Append(msgStr).ToString();
        }
    }
}