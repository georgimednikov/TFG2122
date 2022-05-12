namespace Telemetry.Events
{
    public class CreatureAdult : CreatureEvent
    {
        public bool Original { get; private set; }
        public CreatureAdult(int tick, int creatureID, string speciesName, int x, int y) : base(EventType.CreatureAdult, tick, creatureID, speciesName, x, y) 
        {
            Original = tick == 1;
        }
    }
}
