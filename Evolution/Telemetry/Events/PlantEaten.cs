namespace Telemetry.Events
{
    public class PlantEaten : PlantEvent
    {  
        public PlantEaten(int tick, int creatureID, int x, int y) : base(EventType.PlantEaten, tick, creatureID, x, y) {}
    }
}
