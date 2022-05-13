namespace Telemetry.Events
{
    public class CreaturePositionSample : CreatureEvent
    {
        public CreaturePositionSample(int tick, int id, string species, int x, int y) : base(EventType.CreaturePositionSample, tick, id, species, x, y) { }
    }
}
