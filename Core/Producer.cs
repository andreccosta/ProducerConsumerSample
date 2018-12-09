using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace GenericProducerConsumer.Core
{
    public class Producer<TQueueItem> where TQueueItem : class
    {
        public string Id { get; private set; }

        private Func<ICollection<TQueueItem>> action;

        private readonly ManagedQueue<TQueueItem> queue;
        private readonly CancellationToken cancellationToken;

        public Producer(ManagedQueue<TQueueItem> queue, Func<ICollection<TQueueItem>> action, CancellationToken cancellationToken)
        {
            this.Id = Guid.NewGuid().ToString("N");

            this.action = action;
            this.queue = queue;
            this.cancellationToken = cancellationToken;
        }

        public void Run()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine($"Producer #{Id} - Running ...");

                var itemsToEnqueue = action();

                queue.Enqueue(itemsToEnqueue);

                Thread.Sleep(10000);
            }

            Console.WriteLine($"Producer #{Id} - Done");
        }
    }
}
