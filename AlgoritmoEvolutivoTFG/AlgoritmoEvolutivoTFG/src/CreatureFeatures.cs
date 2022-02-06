namespace EvolutionSimulation.src
{
    //16 in total
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

        //Must be the last attribute and not used
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