namespace Telemetry.Events
{
    public class SimulationSample : TrackerEvent
    {
        public SimulationSample(int tick, int numCreatures) : base(EventType.SimulationSample) { Tick = tick; NumCreatures = numCreatures; }
        public int Tick { get; private set; }
        public int NumCreatures { get; private set; }
    }
}
