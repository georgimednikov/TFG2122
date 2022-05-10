namespace Telemetry.Events
{
    public class CreatureStateEntryNotSafe : CreatureStateEntry
    {  
        public int RivalID { get; private set; }
        public string RivalSpeciesName { get; private set; }
        public CreatureStateEntryNotSafe(int tick, int creatureID, string speciesName, string state, int rivalID, int x, int y, string rivalSpeciesName = "") : base(tick, creatureID, speciesName, state, x, y) { RivalID = rivalID; RivalSpeciesName = rivalSpeciesName; }

    }
}
