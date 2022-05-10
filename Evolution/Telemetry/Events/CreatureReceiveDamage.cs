using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Telemetry.Events
{
    public class CreatureReceiveDamage : CreatureEvent
    {
        public int attackerID { get; private set; }
        public float damage { get; private set; }
        public double remainingHP { get; private set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DamageType damageType { get; private set; }
        public CreatureReceiveDamage(int tick, int creatureID, string speciesName, int aID, float dmg, DamageType dT, double rHP, int x, int y) : base(EventType.CreatureReceiveDamage, tick, creatureID, speciesName, x, y) 
        { attackerID = aID; damage = dmg; damageType = dT; remainingHP = rHP; }
    }
}
