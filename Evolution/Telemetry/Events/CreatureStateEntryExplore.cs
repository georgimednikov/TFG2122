namespace Telemetry.Events
{
    public class CreatureStateEntryExplore : CreatureStateEntry
    {  
        public string ResourceNeeded { get; private set; }
        public CreatureStateEntryExplore(int tick, int creatureID, string speciesName, string state, string resourceNeeded, int x, int y) : base(tick, creatureID, speciesName, state, x, y) { ResourceNeeded = resourceNeeded; }

    }
}
