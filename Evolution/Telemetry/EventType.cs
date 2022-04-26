using System.Runtime.Serialization;

namespace Telemetry
{
    public enum EventType
    {
        [EnumMember(Value = "SesionStart")]
        SessionStart,
        [EnumMember(Value = "SimulationSample")]
        SimulationSample,
        [EnumMember(Value = "SesionEnd")]
        SessionEnd
    }
}
