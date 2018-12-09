using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GenericProducerConsumer.Core
{
    public class ManagedQueue<TQueueItem> where TQueueItem : class
    {
        private int maxConsumers;
        private readonly ConcurrentQueue<TQueueItem> queue = new ConcurrentQueue<TQueueItem>();

        private readonly ConcurrentDictionary<int, Task> tasks = new ConcurrentDictionary<int, Task>();
        private readonly ConcurrentDictionary<int, Consumer<TQueueItem>> consumers = new ConcurrentDictionary<int, Consumer<TQueueItem>>();
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private readonly Action<TQueueItem> consumerAction;
        private readonly Func<ICollection<TQueueItem>> producerAction;

        public ManagedQueue(int maxConsumers, Func<ICollection<TQueueItem>> producerAction, Action<TQueueItem> consumerAction)
        {
            this.maxConsumers = maxConsumers;

            this.producerAction = producerAction;
            this.consumerAction = consumerAction;
        }

        public void Start()
        {
            StartProducer();
        }

        public void Stop()
        {
            // Signal producer and all consumers to stop
            cancellationTokenSource.Cancel();

            // Wait for all
            Task.WaitAll(tasks.Values.ToArray());
        }

        public void Enqueue(TQueueItem queueItem)
        {
            queue.Enqueue(queueItem);

            StartConsumers(1);
        }

        public void Enqueue(ICollection<TQueueItem> queueItems)
        {
            // Add items to internal queue
            foreach (var item in queueItems)
            {
                queue.Enqueue(item);
            }

            StartConsumers(queueItems.Count);
        }

        public bool TryDequeue(out TQueueItem queueItem)
        {
            return queue.TryDequeue(out queueItem);
        }

        private void StartConsumer()
        {
            var consumer = new Consumer<TQueueItem>(this, consumerAction, cancellationTokenSource.Token);
            var consumerTask = new Task(consumer.Run);

            // Remove stopping consumer from tasks dictionary
            consumerTask.ContinueWith(t =>
            {
                consumers.TryRemove(t.Id, out Consumer<TQueueItem> _);
                tasks.TryRemove(t.Id, out Task _);
            });

            // Add starting consumer to tasks dictionary
            consumers.TryAdd(consumerTask.Id, consumer);
            tasks.TryAdd(consumerTask.Id, consumerTask);

            // Start consumer
            consumerTask.Start();
        }

        private void StartConsumers(int count)
        {
            // If the number of wanted new consumers plus the current consumers count exceedes capacity then limit
            if (count + consumers.Count > maxConsumers)
            {
                count = maxConsumers - consumers.Count;
            }

            for (int i = 0; i < count; i++)
            {
                StartConsumer();
            }
        }

        private void StartProducer()
        {
            var producer = new Producer<TQueueItem>(this, producerAction, cancellationTokenSource.Token);
            var producerTask = new Task(producer.Run);

            // Remove stopping producer from tasks dictionary
            producerTask.ContinueWith(t =>
            {
                tasks.TryRemove(t.Id, out Task _);
            });

            // Add starting producer to tasks dictionary
            tasks.TryAdd(producerTask.Id, producerTask);

            // Start producer
            producerTask.Start();
        }
    }
}
