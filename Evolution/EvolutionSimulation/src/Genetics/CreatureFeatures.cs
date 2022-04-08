using System.Runtime.Serialization;

namespace EvolutionSimulation.Genetics
{
    public enum CreatureFeature
    {
        //EnumMembers to read the actual name in the deserialization of the json instead of a number

        //Attributes
        [EnumMember(Value = "Strength")]
        Strength = 0,
        [EnumMember(Value = "Constitution")]
        Constitution,
        [EnumMember(Value = "Fortitude")]
        Fortitude,
        [EnumMember(Value = "Mobility")]
        Mobility,
        [EnumMember(Value = "Resistence")]
        Resistance,
        [EnumMember(Value = "Perception")]
        Perception,
        [EnumMember(Value = "Knowledge")]
        Knowledge,
        [EnumMember(Value = "Camouflage")]
        Camouflage,
        [EnumMember(Value = "Size")]
        Size,
        [EnumMember(Value = "Piercing")]
        Piercing,
        [EnumMember(Value = "Aggressiveness")]
        Aggressiveness,
        [EnumMember(Value = "Metabolism")]
        Metabolism,
        [EnumMember(Value = "IdealTemperature")]
        IdealTemperature,
        [EnumMember(Value = "TemperatureRange")]
        TemperatureRange,
        [EnumMember(Value = "Longevity")]
        Longevity,
        [EnumMember(Value = "Diet")]
        Diet,
        [EnumMember(Value = "Members")] //TODO: limbs no members
        Members,

        //Abilities
        [EnumMember(Value = "Arboreal")]
        Arboreal,
        [EnumMember(Value = "Wings")] //TODO: Mejor aereal que wings
        Wings,
        [EnumMember(Value = "Venomous")]
        Venomous,
        [EnumMember(Value = "NightVision")]
        NightVision,
        [EnumMember(Value = "Horns")]
        Horns,
        [EnumMember(Value = "Mimic")] //TODO: Mimicry no mimic
        Mimic,
        [EnumMember(Value = "Upright")]
        Upright,
        [EnumMember(Value = "Thorns")]
        Thorns,
        [EnumMember(Value = "Scavenger")]
        Scavenger,
        [EnumMember(Value = "Hair")]
        Hair,
        [EnumMember(Value = "Paternity")]
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