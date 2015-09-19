using Logging.ThriftContract;
using System;
using System.Collections.Generic;
using System.Configuration;
using Thrift.Protocol;
using Thrift.Transport;
using System.Linq;

namespace Logging.Client
{
    /// <summary>
    /// Thrift Http协议发送消息
    /// </summary>
    internal class THttpLogSender : LogSenderBase
    {

        static string loggingServerHost = ConfigurationManager.AppSettings["LoggingServerHost"] ?? Settings.LoggingServerHost;
        static Uri uri = new Uri(loggingServerHost + "/Reciver.ashx");

        public override void Send(IList<ILogEntity> logEntities)
        {
            if (logEntities == null || logEntities.Count <= 0) { return; }


            TMsg tmsg = this.CreateTMsg(logEntities);

            var httpClient = new THttpClient(uri);
            httpClient.ConnectTimeout = SENDER_TIMEOUT;
            //var protocol = new TBinaryProtocol(httpClient);
            var protocol = new TCompactProtocol(httpClient);
            httpClient.Open();
            var client = new LogTransferService.Client(protocol);
            client.Log(tmsg);
            httpClient.Close();
        }
    }
}