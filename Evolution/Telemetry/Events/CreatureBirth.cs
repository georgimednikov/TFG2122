namespace Telemetry.Events
{
    public class CreatureBirth : CreatureEvent
    {
        public float MaxHealth;
        public float HealthRegen;
        public float MaxEnergy;
        public float EnergyExpense;
        public float MaxHydration;
        public float HydrationExpense;
        public float MaxRest;
        public float RestExpense;
        public float RestRecovery;
        public int MaxPerception;
        public double MinTemperature;
        public double MaxTemperature;

        public string Gender;
        public string Diet;
        public int Damage;
        public int Armor;
        public int Perforation;
        public float Venom;
        public float Counter;
        public int GroundSpeed;
        public int AerialSpeed;
        public bool AirReach;
        public int ArborealSpeed;
        public bool TreeReach;
        public int Camouflage;
        public int Aggressiveness;
        public int Intimidation;
        public int Size;
        public int LifeSpan;
        public int Limbs;
        public int Metabolism;
        public bool Hair;
        public int Knowledge;
        public int Paternity;
        public bool Upright;

        public CreatureBirth(int tick, int id, string species, int x, int y) : base(EventType.CreatureBirth, tick, id, species, x, y) { }
        public override string ToJSON() { return $"[\n{base.ToJSON()}"; }        
    }
}
