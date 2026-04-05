using System;
using System.Collections.Generic;

namespace ProjectSMP.Core.Discords
{
    public static class DiscordEventBus
    {
        private static readonly Dictionary<string, List<Action<object>>> _subscribers = new();

        public static void Subscribe(string eventName, Action<object> handler)
        {
            if (!_subscribers.ContainsKey(eventName))
                _subscribers[eventName] = new List<Action<object>>();

            _subscribers[eventName].Add(handler);
        }

        public static void Publish(string eventName, object data)
        {
            if (!_subscribers.TryGetValue(eventName, out var handlers)) return;

            foreach (var handler in handlers)
            {
                try { handler.Invoke(data); }
                catch (Exception ex) { Console.WriteLine($"[DiscordEventBus] Error: {ex.Message}"); }
            }
        }

        public static void Unsubscribe(string eventName, Action<object> handler)
        {
            if (!_subscribers.TryGetValue(eventName, out var handlers)) return;
            handlers.Remove(handler);
        }

        public static void Clear()
        {
            _subscribers.Clear();
        }
    }
}