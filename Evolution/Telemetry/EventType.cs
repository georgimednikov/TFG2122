using System.Runtime.Serialization;

namespace Telemetry
{
    public enum EventType
    {
        [EnumMember(Value = "SessionStart")]
        SessionStart,
        [EnumMember(Value = "SimulationSample")]
        SimulationSample,
        [EnumMember(Value = "CreatureBirth")]
        CreatureBirth,
        [EnumMember(Value = "CreatureStateEntry")]
        CreatureStateEntry,
        [EnumMember(Value = "CreatureAttack")]
        CreatureAttack,
        [EnumMember(Value = "CreatureApplyPoison")]
        CreatureApplyPoison,
        [EnumMember(Value = "CreatureReceiveDamage")]
        CreatureReceiveDamage,
        [EnumMember(Value = "CreatureAdult")]
        CreatureAdult,
        [EnumMember(Value = "CreatureMating")]
        CreatureMating,
        [EnumMember(Value = "CreatureDeath")]
        CreatureDeath,
        [EnumMember(Value = "PlantEaten")]
        PlantEaten,
        [EnumMember(Value = "SessionEnd")]
        SessionEnd
    }

    public enum DeathType
    {
        [EnumMember(Value = "Temperature")]
        Temperature,
        [EnumMember(Value = "Attack")]
        Attack,
        [EnumMember(Value = "Retalliation")]
        Retalliation,
        [EnumMember(Value = "Starved")]
        Starved,
        [EnumMember(Value = "Dehydration")]
        Dehydration,
        [EnumMember(Value = "Exhaustion")]
        Exhaustion,
        [EnumMember(Value = "Poisoned")]
        Poisoned,
        [EnumMember(Value = "Longevity")]
        Longevity,
        [EnumMember(Value = "SimulationEnd")]
        SimulationEnd
    }

    public enum DamageType
    {
        [EnumMember(Value = "Temperature")]
        Temperature,
        [EnumMember(Value = "Attack")]
        Attack,
        [EnumMember(Value = "Retalliation")]
        Retalliation,
        [EnumMember(Value = "Starvation")]
        Starvation,
        [EnumMember(Value = "Dehydration")]
        Dehydration,
        [EnumMember(Value = "Exhaustion")]
        Exhaustion,
        [EnumMember(Value = "Poison")]
        Poison
    }
}
