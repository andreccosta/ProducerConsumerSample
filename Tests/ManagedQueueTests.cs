using System;
using System.Collections.Generic;
using System.Threading;
using GenericProducerConsumer.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenericProducerConsumer.Tests
{
    [TestClass]
    public class ManagedQueueTests
    {
        private class WorkItem
        {
            public WorkItem(int value)
            {
                this.Value = value;
            }
            public int Value { get; set; }
        }

        [TestMethod]
        public void ManagedQueue_WithSingleWorker_Ok()
        {
            // Shared results
            List<WorkItem> results = new List<WorkItem>();

            Func<ICollection<WorkItem>> producerAction = () =>
            {
                List<WorkItem> initialValues = new List<WorkItem> {
                    new WorkItem(10),
                    new WorkItem(20),
                    new WorkItem(30),
                    new WorkItem(40)
                };

                return initialValues;
            };

            Action<WorkItem> consumerAction = (item) =>
            {
                results.Add(new WorkItem(item.Value + 2));
            };

            var queue = new ManagedQueue<WorkItem>(1, producerAction, consumerAction);

            queue.Start();

            // TODO: Remove thread sleep. Add logic to check queue status and wait for it to finish
            Thread.Sleep(300);

            queue.Stop();

            Assert.IsTrue(results.Count > 0);
        }
    }
}
