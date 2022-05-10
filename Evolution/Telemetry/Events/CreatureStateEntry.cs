namespace Telemetry.Events
{
    public class CreatureStateEntry : CreatureEvent
    {  
        public string State { get; private set; }
        public CreatureStateEntry(int tick, int creatureID, string speciesName, string state, int x, int y) : base(EventType.CreatureStateEntry, tick, creatureID, speciesName, x, y) { State = state; }

    }
}
