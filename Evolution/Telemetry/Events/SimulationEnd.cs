namespace Telemetry.Events
{
    public class SimulationEnd : TrackerEvent
    {
       
        public int TicksSimulated { get; private set; }
        public int NumCreaturesAlive { get; private set; }
        public int NumSpeciesAlive { get; private set; }
        public SimulationEnd(int ticksSimulated, int numCreaturesAlive, int numSpeciesAlive) : base(EventType.SimulationEnd)
        {
            TicksSimulated = ticksSimulated; NumCreaturesAlive = numCreaturesAlive; NumSpeciesAlive = numSpeciesAlive;
        }
    }
}
