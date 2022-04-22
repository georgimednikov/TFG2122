using System;
using System.Threading;
using Telemetry;
using Telemetry.Events;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Tracker.Instance.Init();
            Tracker.Instance.Track(new SessionStart());
            Thread.Sleep(1000);
            Tracker.Instance.Track(new SimulationSample(500));
            Thread.Sleep(1000);
            Tracker.Instance.Track(new SessionEnd());
            Tracker.Instance.Flush();
        }
    }
}
