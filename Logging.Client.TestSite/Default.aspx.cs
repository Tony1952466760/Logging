﻿using System;
using System.Collections.Generic;

namespace PLU.Logging.Client.TestSite
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ILog logger = LogManager.GetLogger(typeof(Default));

            logger.Debug("test");
            logger.Info("aabbbbbcc", "test");
            logger.Warm("test", "大大的打算打算大大", null);
            Dictionary<string,string> tags=new Dictionary<string,string>();
            tags.Add("a","a");
            tags.Add("adsadadad", "aweqweqweqe2312313gdgdfgdg!@##$%");
            tags.Add("a阿迪达sad", "aweqweqweq打啊打多大的萨达大厦e2312313gdgdfgdg!@##$%");
            tags.Add("a阿迪达2424sad", @"aweqweqweq打啊打多大的萨达大厦e2312313gdgdfgdga
                weqweqweq打啊打多大的萨达大厦e2312313gdgdfgdgaweqweqweq打啊打多大的
                萨达大厦e2312313gdgdfgdgaweqwwrqwq付费方式是放松放
                萨达大厦e2312313gdgdfgdgaweqwwrqwq付费方式是放松放
                萨达大厦e2312313gdgdfgdgaweqwwrqwq付费方式是放松放
                萨达大厦e2312313gdgdfgdgaweqwwrqwq付费方式是放松放
                松放松放松放松eqweq打啊打多大的萨达大厦e2312313gdgdfgdg!@##$%");
            logger.Error("test","test",tags);


            //try
            //{
            //    throw new Exception("test exception");
            //}
            //catch(Exception ex)
            //{
            //    logger.Error(ex);
            //}
          

            
        }
    }
}