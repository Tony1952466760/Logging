﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PLU.Logging.Client
{
    /// <summary>
    /// 多线程消费队列。将输入元素打包输出。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class TimerBatchBlock<T>
    {
        /// <summary>
        /// 当前队列长度
        /// </summary>
        private int QueueLength { get; set; }

        private Task[] Tasks { get; set; }

        private Action<List<T>> Action { get; set; }

        private BlockingCollection<T> s_Queue;

        /// <summary>
        /// 最近一次异常报告时间
        /// </summary>
        public DateTime LastExceptionReportTime { get; private set; }

        /// <summary>
        /// 出现的异常数量
        /// </summary>
        public int ExceptionCount { get; private set; }

        /// <summary>
        /// 最近一次异常
        /// </summary>
        public Exception LastException { get; private set; }

        /// <summary>
        /// 阻塞队列的最大长度
        /// </summary>
        private int QueueMaxLength { get; set; }

        private ConcurrentBag<T> Batch { get; set; }

        /// <summary>
        /// 元素包的大小
        /// </summary>
        private int BatchSize { get; set; }

        /// <summary>
        /// 上一次打包处理的时间
        /// </summary>
        private DateTime LastActionTime { get; set; }

        private int BlockElapsed { get; set; }

        /// <summary>
        /// 多线程消费队列
        /// </summary>
        /// <param name="taskNum">处理队列出队的线程数量</param>
        /// <param name="action">处理委托</param>
        /// <param name="queueMaxLength">设置队列最大长度</param>
        /// <param name="batchSize">元素包的大小</param>
        /// <param name="blockElapsed">阻塞的时间，达到该时间间隔，也会出队</param>
        public TimerBatchBlock(int taskNum, Action<List<T>> action, int queueMaxLength, int batchSize, int blockElapsed)
        {
            s_Queue = new BlockingCollection<T>();
            Batch = new ConcurrentBag<T>();
            this.LastActionTime = DateTime.Now;
            this.LastExceptionReportTime = DateTime.Now;
            this.BatchSize = batchSize;
            this.BlockElapsed = blockElapsed;
            this.Action = action;
            this.QueueMaxLength = queueMaxLength;
            this.Tasks = new Task[taskNum];
            for (int i = 0; i < taskNum; i++)
            {
                int temp_i = i;
                this.Tasks[temp_i] = Task.Factory.StartNew(this.DequeueProcess);
            }
        }

        /// <summary>
        /// 入队处理
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(T item)
        {
            int queueLen = s_Queue.Count;
            if (queueLen >= this.QueueMaxLength)
            {
                for (int i = 0; i < (queueLen - this.QueueMaxLength) + 1; i++)
                {
                    // T removedItem;
                    // this.s_Queue.TryDequeue(out removedItem);
                    this.s_Queue.Take();
                }
            }
            // this.s_Queue.Enqueue(item);
            this.s_Queue.Add(item);
        }

        /// <summary>
        /// 出队处理函数
        /// </summary>
        private void DequeueProcess()
        {
            while (true)
            {
                try
                {
                    T item;
                    bool hasItem = s_Queue.TryTake(out item, 200);

                    if (hasItem)
                    {
                        this.Batch.Add(item);
                    }

                    if (this.Batch.Count > 0)
                    {
                        var _now = DateTime.Now;
                        var elapsed = (_now - this.LastActionTime).TotalMilliseconds;
                        if ((this.Batch.Count >= this.BatchSize || elapsed > this.BlockElapsed))
                        {
                            this.Action(this.Batch.ToList());
                            this.Batch = new ConcurrentBag<T>();
                            this.LastActionTime = DateTime.Now;
                        }
                    }
                }
                catch (ThreadAbortException tae)
                {
                    Thread.ResetAbort();

                    this.ExceptionCount += 1;
                    this.LastException = tae;
                }
                catch (Exception ex)
                {
                    this.ExceptionCount += 1;
                    this.LastException = ex;
                }
                finally
                {
                    this.ReportException();
                }
            }
        }

        /// <summary>
        /// 报告PLU.Logging.Client自身异常
        /// </summary>
        private void ReportException()
        {
            var _now = DateTime.Now;
            var exceportElapsed = (_now - this.LastExceptionReportTime).TotalSeconds;
            if (exceportElapsed >= 60)
            {
                LogExceptionHandller.WriteLog(this.LastException, this.ExceptionCount);
                this.ExceptionCount = 0;
                this.LastException = null;
                this.LastExceptionReportTime = DateTime.Now;
            }
        }
    }
}