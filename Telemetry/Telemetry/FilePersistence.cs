using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Telemetry
{
    public class FilePersistence : IPersistence
    {
        public FilePersistence()
        {
            serializer = new JSONSerializer();
            eventQueue = new Queue<TrackerEvent>();
        }
        
        public void Send(TrackerEvent tEvent)
        {
            eventQueue.Enqueue(tEvent);
        }

        public void Flush()
        {
            if (!Directory.Exists("Output"))
                Directory.CreateDirectory("Output");

            using (StreamWriter writer = new StreamWriter($"Output/{Tracker.Instance.SessionID}.json"))
            {
                while(eventQueue.Count > 0)
                {
                    TrackerEvent tEvent = eventQueue.Dequeue();
                    writer.WriteLine(serializer.Serialize(tEvent));
                }
            }
        }

        ISerializer serializer;
        Queue<TrackerEvent> eventQueue;
    }
}
