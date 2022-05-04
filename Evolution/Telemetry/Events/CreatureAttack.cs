namespace Telemetry.Events
{
    public class CreatureAttack : CreatureEvent
    {
        public int victimID { get; private set; }
        public string victimSpecies { get; private set; }
        public int damage { get; private set; }
        public int penetration { get; private set; }
        public CreatureAttack(int tick, int creatureID, string speciesName, int vID, string vSp, int dmg, int pen) : base(EventType.CreatureAttack, tick, creatureID, speciesName) 
        { victimID = vID; victimSpecies = vSp; damage = dmg; penetration = pen; }
    }
}
