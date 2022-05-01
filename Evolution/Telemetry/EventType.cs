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
        [EnumMember(Value = "CreatureDeath")]
        CreatureDeath,
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
        Poisoned
    }
}
