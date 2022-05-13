namespace Telemetry.Events
{
    public class SimulationStart : TrackerEvent
    {
        public int YearTicks { get; private set; }
        public int TotalTicks { get; private set; }
        public int TotalEdiblePlants { get; private set; }
        public int MapSize { get; private set; }

        public SimulationStart(int yearTicks, int totalTicks, int totalEdiblePlants, int mapSize) : base(EventType.SimulationStart)
        {
            YearTicks = yearTicks; TotalTicks = totalTicks; TotalEdiblePlants = totalEdiblePlants; MapSize = mapSize;
        }
    }
}
