using Logging.ThriftContract;
using System;
using System.Collections.Generic;
using System.Configuration;
using Thrift.Protocol;
using Thrift.Transport;

namespace Logging.Client
{
    /// <summary>
    /// Thrift Socket协议发送消息
    /// </summary>
    internal class TSocketLogSender : LogSenderBase
    {
        public override void Send(IList<ILogEntity> logEntities)
        {
            if (logEntities == null || logEntities.Count <= 0) { return; }
            TMsg tmsg = this.CreateTMsg(logEntities);

            var socket = new TSocket("localhost", 9813);
            socket.Timeout = SENDER_TIMEOUT;
            var transport = new TFramedTransport(socket);
            var protocol = new TCompactProtocol(transport);
            var client = new LogTransferService.Client(protocol);
            transport.Open();

        
            client.Log(tmsg);
            transport.Close();
           
        }
    }
}