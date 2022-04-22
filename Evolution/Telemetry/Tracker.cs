using System;
using System.Diagnostics;

namespace Telemetry
{
    public class Tracker
    {
        static public Tracker Instance {
            get
            {
                if (_instance == null)
                    _instance = new Tracker();
                return _instance;
            }
        }
        static private Tracker _instance;

        private Tracker() {}

        public void Init()
        {
            persistence = new FilePersistence();
            SessionID = Guid.NewGuid().ToString("N");
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        public void Track(TrackerEvent tEvent)
        {
            persistence.Send(tEvent);
        }

        public void Flush()
        {
            persistence.Flush();
        }

        public long GetSessionTimestamp()
        {
            return stopwatch.ElapsedMilliseconds;
        }

        public string SessionID { get; private set; }
        private IPersistence persistence;
        private Stopwatch stopwatch;
    }
}
