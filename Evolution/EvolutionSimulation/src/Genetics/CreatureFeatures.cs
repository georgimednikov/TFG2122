namespace EvolutionSimulation.Genetics
{
    public enum CreatureFeature
    {
        //Attributes
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
        IdealTemperature,
        TemperatureRange,
        Longevity,
        Diet,
        Members,

        //Abilities
        Arboreal,
        Wings,
        Venomous,
        NightVision,
        Horns,
        Mimic,
        Upright,
        Thorns,
        Scavenger,
        Hair,
        Paternity,

        //Total features
        Count
    }

    public enum Gender
    {
        Male,
        Female
    }

    public enum Diet
    {
        Herbivore,
        Omnivore,
        Carnivore,

        //Total diets
        Count
    }
}