namespace Telemetry.Events
{
    public class CreatureStateEntryExplore : CreatureStateEntry
    {  
        public string ResourceNeeded { get; private set; }
        public CreatureStateEntryExplore(int tick, int creatureID, string speciesName, string state, string resourceNeeded) : base(tick, creatureID, speciesName, state) { ResourceNeeded = resourceNeeded; }

    }
}
