using System.Collections.Generic;
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
            lock (eventQueue)
            {
                eventQueue.Enqueue(tEvent);
            }
        }

        public void Flush()
        {
            if (!Directory.Exists("Output"))
                Directory.CreateDirectory("Output");

            using (StreamWriter writer = File.AppendText($"Output/{Tracker.Instance.SessionID}.json"))
            {
                lock (eventQueue)
                {
                    while (eventQueue.Count > 0)
                    {
                        TrackerEvent tEvent = eventQueue.Dequeue();
                        writer.WriteLine(serializer.Serialize(tEvent));
                    }
                }
            }
        }

        ISerializer serializer;
        Queue<TrackerEvent> eventQueue;
    }
}
