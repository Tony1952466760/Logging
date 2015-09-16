﻿using System;
using System.Collections.Generic;

namespace PLU.Logging.Client
{
    /// <summary>
    /// 处理日志过程中出现的异常
    /// </summary>
    internal class LogExceptionHandller
    {
        private static ILog logger = LogManager.GetLogger(typeof(LogExceptionHandller));

        public static void WriteLog(Exception ex, int count)
        {
            string msg = "最近一分钟该应用内(" + Settings.AppId + ")PLU.Logging.Client发生" + count + "条异常数量";
            msg += "</br>";
            if (count > 0)
            {
                msg += "最近一条异常:" + ex.ToString();
                var tags = new Dictionary<string, string>();
                tags.Add("type", "one_minute_err");
                logger.Error("Logging_Client_Report", msg, tags);
            }
        }
    }
}