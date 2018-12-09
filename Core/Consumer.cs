using System;
using System.Collections.Concurrent;
using System.Threading;

namespace GenericProducerConsumer.Core
{
    public class Consumer<TQueueItem> where TQueueItem : class
    {
        public string Id { get; private set; }

        private Action<TQueueItem> action;

        private readonly ManagedQueue<TQueueItem> queue;
        private readonly CancellationToken cancellationToken;

        public Consumer(ManagedQueue<TQueueItem> queue, Action<TQueueItem> action, CancellationToken cancellationToken)
        {
            this.Id = Guid.NewGuid().ToString("N");

            this.action = action;
            this.queue = queue;
            this.cancellationToken = cancellationToken;
        }

        public void Run()
        {
            while (!cancellationToken.IsCancellationRequested && queue.TryDequeue(out TQueueItem queueItem))
            {
                Console.WriteLine($"Consumer #{Id} - Dequeued {queueItem}");

                action(queueItem);

                Thread.Sleep(500);
            }

            Console.WriteLine($"Consumer #{Id} - Queue is empty. Done");
        }
    }
}
