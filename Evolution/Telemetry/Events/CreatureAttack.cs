using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Telemetry.Events
{
    public class CreatureAttack : CreatureEvent
    {
        public int victimID { get; private set; }
        public int damage { get; private set; }
        public int penetration { get; private set; }
        public CreatureAttack(int tick, int creatureID, string speciesName, int vID, int dmg, int pen) : base(EventType.CreatureAttack, tick, creatureID, speciesName) 
        { victimID = vID; damage = dmg; penetration = pen; }
        public override string ToJSON()
        {
            string aux = base.ToJSON();
            return $"{aux.Remove(aux.Length - 1)}\n]";
        }
    }
}
