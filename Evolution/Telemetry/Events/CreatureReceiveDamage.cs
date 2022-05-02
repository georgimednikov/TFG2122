using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Telemetry.Events
{
    public class CreatureReceiveDamage : CreatureEvent
    {
        public int attackerID { get; private set; }
        public float damage { get; private set; }
        public double remainingHP { get; private set; }
        public DamageType damageType { get; private set; }
        public CreatureReceiveDamage(int tick, int creatureID, string speciesName, int aID, float dmg, DamageType dT, double rHP) : base(EventType.CreatureReceiveDamage, tick, creatureID, speciesName) 
        { attackerID = aID; damage = dmg; damageType = dT; remainingHP = rHP; }
        public override string ToJSON()
        {
            string aux = base.ToJSON();
            return $"{aux.Remove(aux.Length - 1)}\n]";
        }
    }
}
