namespace EvolutionSimulation.src
{
    public struct CreatureFeatures
    {
        static public int Attributes = 16;
        static public int Abilities = 11;
        static public int Features = Attributes + Abilities;

        public enum CreatureAttribute
        {
            Strength = 0,
            Constitution,
            Fortitude,
            Mobility,
            Resistence,
            Perception,
            Knowledge,
            Camouflage,
            Size,
            Piercing,
            Aggressiveness,
            Metabolism,
            BodyTemperature,
            Longevity,
            Diet,
            Members,

            Last
        }

        //Starts after the attributes
        public enum CreatureAbility
        {
            Arboreal = CreatureAttribute.Last,
            Wings,
            Venomous,
            NightVision,
            Horns,
            Mimic,
            Upright,
            Thorns,
            Scavenger,
            Hair,
            Paternity
        }
    }
}