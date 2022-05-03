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
            //lock (eventQueue)
            //{
                eventQueue.Enqueue(tEvent);
            //}
        }

        public void Flush()
        {
            string directoryPath = $"Output/{Tracker.Instance.SessionID}";
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            int numEvents = eventQueue.Count;
            using (StreamWriter writer = File.AppendText($"Output/{Tracker.Instance.SessionID}/sessionOutput.json"))
            {
               
                //lock (eventQueue)
                //{
                    while (eventQueue.Count > 0)
                    {
                        TrackerEvent tEvent = eventQueue.Dequeue();
                        if(tEvent is Events.CreatureEvent)
                        {
                            Events.CreatureEvent cEvent = tEvent as Events.CreatureEvent;
                            if (!Directory.Exists($"{directoryPath}/{cEvent.Species}"))
                                Directory.CreateDirectory($"{directoryPath}/{cEvent.Species}");
                            using (StreamWriter cWriter = File.AppendText($"{directoryPath}/{cEvent.Species}/{cEvent.CreatureID}.json"))
                            {
                                cWriter.WriteLine(serializer.Serialize(cEvent));
                            }
                        }
                        else
                            writer.WriteLine(serializer.Serialize(tEvent));                      
                    }
                //}
            }
#if DEBUG
            System.Console.WriteLine($"{numEvents} Events flushed !");
#endif
        }

        ISerializer serializer;
        Queue<TrackerEvent> eventQueue;
    }
}
