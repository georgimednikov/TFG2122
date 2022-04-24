namespace Telemetry
{
    interface ISerializer
    {
        string Serialize(TrackerEvent tEvent);
    }
}
