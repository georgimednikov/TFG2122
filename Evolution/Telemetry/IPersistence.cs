namespace Telemetry
{
    interface IPersistence
    {
        void Send(TrackerEvent tEvent);
        void Flush();
    }
}
