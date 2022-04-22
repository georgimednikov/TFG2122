using System;
using System.Collections.Generic;
using System.Text;

namespace Telemetry.Events
{
    public class SimulationSample : TrackerEvent
    {
        public SimulationSample(int tick) : base(EventType.SimulationSample) { Tick = tick; }
        public int Tick { get; private set; }
    }
}
