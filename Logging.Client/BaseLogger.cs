﻿using PLU.Logging.Client.LogSender;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PLU.Logging.Client
{
    internal abstract class BaseLogger : ILog
    {

        private string Source { get; set; }

        public BaseLogger(string source = "")
        {
            this.Source = source;
        }

        static BaseLogger()
        {
            int LoggingTaskNum = Convert.ToInt32(ConfigurationManager.AppSettings["LoggingTaskNum"] ?? Settings.LoggingTaskNum.ToString());

            int LoggingQueueLength = Convert.ToInt32(ConfigurationManager.AppSettings["LoggingQueueLength"] ?? Settings.LoggingQueueLength.ToString());

            int LoggingBatchSize = Convert.ToInt32(ConfigurationManager.AppSettings["LoggingBatchSize"] ?? Settings.LoggingBatchSize.ToString());

            int LoggingBlockElapsed = Convert.ToInt32(ConfigurationManager.AppSettings["LoggingBlockElapsed"] ?? Settings.LoggingBlockElapsed.ToString());

            if (LoggingTaskNum <= 0) { LoggingTaskNum = Settings.DefaultLoggingTaskNum; }

            if (LoggingQueueLength <= 0) { LoggingQueueLength = Settings.DefaultLoggingQueueLength; }

            if (LoggingBatchSize <= 0) { LoggingBatchSize = Settings.DefaultLoggingBatchSize; }

            if (LoggingBlockElapsed <= 0) { LoggingBlockElapsed = Settings.DefaultLoggingBlockElapsed; }

            block = new TimerBatchBlock<LogEntity>(LoggingTaskNum, (batch) =>
            {
                LogSenderBase sender = LogSenderManager.GetLogSender();
                sender.Send(batch);
            }, LoggingQueueLength, LoggingBatchSize, LoggingBlockElapsed);
        }

        public void Debug(string message)
        {
            Debug(string.Empty, message);
        }

        public void Debug(string title, string message)
        {
            Debug(title, message, null);
        }

        public void Debug(string title, string message, Dictionary<string, string> tags)
        {
            Log(title, message, tags, LogLevel.Debug);
        }

        public void Info(string message)
        {
            Info(string.Empty, message, null);
        }

        public void Info(string title, string message)
        {
            Info(title, message, null);
        }

        public void Info(string title, string message, Dictionary<string, string> tags)
        {
            Log(title, message, tags, LogLevel.Info);
        }

        public void Warm(string message)
        {
            Warm(string.Empty, message);
        }

        public void Warm(string title, string message)
        {
            Warm(title, message, null);
        }

        public void Warm(string title, string message, Dictionary<string, string> tags)
        {
            Log(title, message, tags, LogLevel.Warm);
        }

        public void Error(string message)
        {
            Error(string.Empty, message, null);
        }

        public void Error(string title, string message)
        {
            Error(title, message, null);
        }

        public void Error(string title, string message, Dictionary<string, string> tags)
        {
            Log(title, message, tags, LogLevel.Error);
        }

        public void Error(Exception ex)
        {
            this.Error(ex.Message, ex.ToString());
        }

        protected LogEntity CreateLog(string source, string title, string message, Dictionary<string, string> tags, LogLevel level)
        {
            LogEntity log = new LogEntity();
            log.IP = ServerIPNum;
            log.Level = level;
            log.Message = message;
            log.Tags = tags;
            log.Time = Utils.GetTimeStamp(DateTime.Now);
            log.Title = title;
            log.Source = source;
            log.Thread = Thread.CurrentThread.ManagedThreadId;
            log.AppId = Settings.AppId;
            if (log.Tags == null)
            {
                log.Tags = new Dictionary<string, string>();
            }
            return log;
        }

        /// <summary>
        /// 是否禁用日志
        /// </summary>
        protected bool LoggingDisabled
        {
            get
            {
                bool LoggingDisabled = Convert.ToBoolean(ConfigurationManager.AppSettings["LoggingDisabled"] ?? Settings.LoggingDisabled.ToString());
                return LoggingDisabled;
            }
        }

        protected virtual void Log(string title, string message, Dictionary<string, string> tags, LogLevel level)
        {
            if (LoggingDisabled) { return; }
            LogEntity log = this.CreateLog(Source, title, message, tags, level);
            block.Enqueue(log);
        }

        private static TimerBatchBlock<LogEntity> block;


        public string GetLogs(long start, long end, int appId, int[] level = null, string title = "", string msg = "", string source = "", string ip = "", Dictionary<string, string> tags = null, int limit = 100)
        {
            string loggingServerHost = ConfigurationManager.AppSettings["LoggingServerHost"];
            string url = loggingServerHost + "/LogViewer.ashx";

            StringBuilder query = new StringBuilder(url);
            query.Append("?start =" + start);
            query.Append("&");
            query.Append("end=" + end);
            query.Append("&");
            query.Append("appId=" + appId);
            if (level != null && level.Length > 0)
            {
                query.Append("&");
                query.Append("level=" + string.Join(",", level));
            }
            query.Append("&");
            query.Append("title=" + title);
            query.Append("&");
            query.Append("msg=" + msg);
            query.Append("&");
            query.Append("source=" + source);
            query.Append("&");
            query.Append("ip=" + ip);

            if (tags != null && tags.Count > 0)
            {
                string tags_str = string.Empty;
                foreach (var item in tags)
                {
                    tags_str += item.Key + "=" + item.Value;
                    tags_str += ",";
                }
                tags_str = tags_str.TrimEnd(',');
                query.Append("&");
                query.Append("tag=" + tags_str);
            }

            query.Append("&");
            query.Append("limit=" + limit);

            WebClient _client = new WebClient();

            //DownloadData的使用方法
            byte[] resp_byte = _client.DownloadData(query.ToString());
            string resp = Encoding.UTF8.GetString(resp_byte);

            return resp;
        }

        #region 私有成员

        private static long serverIPNum;

        private static long ServerIPNum
        {
            get
            {
                if (serverIPNum <= 0)
                {
                    string serverIP = GetServerIP();
                    serverIPNum = Utils.IPToNumber(serverIP);
                }
                return serverIPNum;
            }
        }

        /// <summary>
        /// 获取服务器IP
        /// </summary>
        /// <returns></returns>
        private static string GetServerIP()
        {
            string str = "127.0.0.1";
            try
            {
                string hostName = Dns.GetHostName();
                var hostEntity = Dns.GetHostEntry(hostName);
                var ipAddressList = hostEntity.AddressList;
                var ipAddress = ipAddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

                if (ipAddress != null)
                {
                    str = ipAddress.ToString();
                }
                return str;
            }
            catch (Exception) { str = string.Empty; }
            return str;
        }
        #endregion
    }
}