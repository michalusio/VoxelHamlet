using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public class MessageAwaiter
    {
        public readonly List<string> Messages = new List<string>();
        public int TickToSend;
    }

    [ExecuteAlways]
    public class DynamicLogger: MonoBehaviour
    {
        [Range(0, 30)]
        public int FramesThrottle = 5;

        private static DynamicLogger instance;

        private readonly Dictionary<string, MessageAwaiter> queuedMessages = new Dictionary<string, MessageAwaiter>();

        void Start()
        {
            if (instance != null && instance != this)
            {
                throw new System.Exception("Cannot have two dynamic loggers!");
            }
            instance = this;
        }

        void Update()
        {
            foreach (var kv in queuedMessages)
            {
                if (kv.Value.TickToSend <= Time.frameCount && kv.Value.Messages.Count > 0)
                {
                    Debug.Log($"[{kv.Key}]\n{string.Join("\n", kv.Value.Messages)}");
                    kv.Value.Messages.Clear();
                }
            }
        }

        public static void Log(string title, params object[] message)
        {
            if (!instance.queuedMessages.ContainsKey(title))
            {
                instance.queuedMessages[title] = new MessageAwaiter();
            }
            var awaiter = instance.queuedMessages[title];
            awaiter.Messages.Add($"* {string.Join(" ", message)}");
            awaiter.TickToSend = Time.frameCount + instance.FramesThrottle;
        }
    }
}
