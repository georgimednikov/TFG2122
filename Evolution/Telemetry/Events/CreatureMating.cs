using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Telemetry.Events
{
    public class CreatureMating : CreatureEvent
    {
        public int MateID { get; private set; }
        public string MateSpecies { get; private set; }
        public int ChildNumber { get; private set; }
        public CreatureMating(int tick, int creatureID, string speciesName, int mateID, string mateSpecies, int childNumber) : base(EventType.CreatureMating, tick, creatureID, speciesName) 
        { MateID = mateID; MateSpecies = mateSpecies; ChildNumber = childNumber; }

    }
}
