namespace Telemetry.Events
{
    public class CreatureAdult : CreatureEvent
    {  
        public CreatureAdult(int tick, int creatureID, string speciesName, int x, int y) : base(EventType.CreatureAdult, tick, creatureID, speciesName, x, y) {}
    }
}
