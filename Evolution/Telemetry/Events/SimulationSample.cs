namespace Telemetry.Events
{
    public class SimulationSample : TrackerEvent
    {
        public SimulationSample(int tick, int numCreatures, float per) : base(EventType.SimulationSample) { Tick = tick; NumCreatures = numCreatures; EatenPlantsRatio = per; }
        public int Tick { get; private set; }
        public int NumCreatures { get; private set; }
        public float EatenPlantsRatio { get; private set; }
    }
}
