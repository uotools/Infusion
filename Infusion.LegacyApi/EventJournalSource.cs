﻿using System;
using System.Collections.Generic;
using System.Threading;
using Infusion.LegacyApi.Events;

namespace Infusion.LegacyApi
{
    internal class EventJournalSource : IEventJournalSource
    {
        private const int MaxEventCount = 2048;

        private readonly Queue<OrderedEvent> events = new Queue<OrderedEvent>();
        private readonly object sourceLock = new object();
        private int counter;
        public event EventHandler<IEvent> NewEventReceived;

        public void Publish(IEvent ev)
        {
            OnNewEventReceived(ev);
        }

        // ReSharper disable once InconsistentlySynchronizedField
        public IEnumerable<OrderedEvent> Events => events;

        public EventId LastEventId { get; private set; }

        private EventId GenerateId() =>
            new EventId(Interlocked.Increment(ref counter));


        private void OnNewEventReceived(IEvent ev)
        {
            lock (sourceLock)
            {
                var id = GenerateId();
                events.Enqueue(new OrderedEvent(id, ev));
                LastEventId = id;

                if (events.Count > MaxEventCount)
                    events.Dequeue();
            }

            NewEventReceived?.Invoke(this, ev);
        }
    }
}