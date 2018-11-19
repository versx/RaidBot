namespace T
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using T.Diagnostics;

    public interface IQueueProcessor<T>
    {
        void Add(T item);
    }

    public abstract class QueueProcessor<T> : IQueueProcessor<T>, IDisposable where T : class
    {
        #region Variables

        private static readonly IEventLogger _logger = EventLogger.GetLogger();

        private readonly Queue<T> _queue;
        private readonly object _locker;

        #endregion

        #region Abstract Methods

        protected abstract Task<bool> ProcessEvent(T item);

        protected abstract void QueueLengthChanged(int length);

        #endregion

        #region Constructor

        protected QueueProcessor()
        {
            _logger.Trace($"QueueProcessor::QueueProcessor");
            _queue = new Queue<T>();
            _locker = new object();
        }

        #endregion

        #region Public Methods

        public void Add(T item)
        {
            _logger.Trace($"QueueProcessor::Add [Item={item}]");

            lock (_locker)
            {
                _queue.Enqueue(item);

                QueueLengthChanged(_queue.Count);
            }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
            var processThread = new Thread(async () => await ProcessPendingQueue());
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
            processThread.Start();
        }

        #endregion

        #region Private Methods

        private async Task ProcessPendingQueue()
        {
            try
            {
                T firstEvent = null;

                if (_queue.Count > 0)
                    firstEvent = _queue.Dequeue();

                if (firstEvent == null)
                    return;

                var success = await ProcessEvent(firstEvent);
                if (!success)
                    return;

                QueueLengthChanged(_queue.Count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        #endregion

        #region IDisposable

        public abstract void Dispose();

        #endregion
    }
}