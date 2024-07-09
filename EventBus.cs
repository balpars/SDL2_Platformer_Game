using System;
using System.Collections.Generic;

namespace Platformer_Game
{
    class EventBus
    {
        private static EventBus instance;
        private Dictionary<string, Action<object>> eventTable;

        private EventBus()
        {
            eventTable = new Dictionary<string, Action<object>>();
        }

        public static EventBus Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EventBus();
                }
                return instance;
            }
        }

        public void Subscribe(string eventType, Action<object> listener)
        {
            if (!eventTable.ContainsKey(eventType))
            {
                eventTable[eventType] = delegate { };
            }
            eventTable[eventType] += listener;
        }

        public void Unsubscribe(string eventType, Action<object> listener)
        {
            if (eventTable.ContainsKey(eventType))
            {
                eventTable[eventType] -= listener;
            }
        }

        public void Publish(string eventType, object parameter = null)
        {
            if (eventTable.ContainsKey(eventType))
            {
                eventTable[eventType].Invoke(parameter);
            }
        }
    }
}
