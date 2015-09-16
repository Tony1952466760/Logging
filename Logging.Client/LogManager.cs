﻿using System;

namespace PLU.Logging.Client
{
    public sealed class LogManager
    {
        private LogManager()
        { }

        /// <summary>
        /// 通过类型名获取ILog实例。
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>ILog instance</returns>
        public static ILog GetLogger(Type type)
        {
            if (type == null)
            {
                return GetLogger("NoName");
            }
            else
            {
                return GetLogger(type.FullName);
            }
        }

        /// <summary>
        /// 通过字符串名获取ILog实例。
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>ILog instance</returns>
        public static ILog GetLogger(string name)
        {
            return new SimpleLogger(name);
        }
    }
}