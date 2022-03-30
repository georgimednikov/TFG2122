using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    class GoToDrink : CreatureState
    {
        Vector2Int ogWaterPos = new Vector2Int(-1, -1);
        Vector2Int waterPosition = new Vector2Int(-1, -1);
        bool notAtDestiny;
        public GoToDrink(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return creature.GetNextCostOnPath();
        }

        public override void OnEntry()
        {
            FindShore();
            ogWaterPos.x = waterPosition.x; ogWaterPos.y = waterPosition.y;
        }

        public override void Action()
        {
            if (waterPosition == creature.WaterPosition() || ogWaterPos != waterPosition)
                FindShore();
            if (notAtDestiny)
            {
                Vector3 nextPos = creature.GetNextPosOnPath();
                //if (nextPos.X < 0) return; //TODO: lo mismo que con los otros gotos
                creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
                notAtDestiny = creature.x != waterPosition.x || creature.y != waterPosition.y || creature.creatureLayer != 0;
            }
        }
        public override string GetInfo()
        {
            return creature.speciesName + " with ID: " + creature.ID + " IN (" + creature.x + ", " + creature.y + ")" + " GOES TO DRINK AT (" + waterPosition.x + ", " + waterPosition.y + ")";
        }

        public override string ToString()
        {
            return "GoToDrinkState";
        }

        private void FindShore()
        {
            ogWaterPos.x = waterPosition.x; ogWaterPos.y = waterPosition.y;
            waterPosition = creature.WaterPosition();


            //Angle between the creatures position and the position of the water from 0 to 180 positive or negative.
            double degrees = Math.Atan2(waterPosition.y - creature.y, waterPosition.x - creature.x) * (180 / Math.PI);

            //Now the sector has to be accounted for.

            //If the resulting angle is negative it is converted to a degree from 0 to 360
            if (degrees < 0) degrees += 360;

            //There are 8 sectors representing the 8 tiles that surround another, each one with an angle of 360/8 = 45 degrees.
            //Half a quadrant of offset to start counting the angle each sector represents from 0.
            degrees += (45.0 / 2) % 360;

            int sector = (int)(degrees / 45);
            int cont = 0;
            Vector2Int originalPos = new Vector2Int(waterPosition);
            while (!creature.world.checkBounds(waterPosition.x, waterPosition.y) ||
                creature.world.map[waterPosition.x, waterPosition.y].isWater)
            {
                //The increment has to have the same sign as cont to add their values without possible substractions,
                //but cont's sign has to be mantained to alternate between going "left" or "right" realtive to the current sector.
                int inc = 1; if (cont < 0) inc *= -1;
                cont = (cont + inc) * -1;
                sector = (sector + cont) % 8;
                if (sector < 0) sector += 8;
                waterPosition = SectorToPosition(originalPos, sector);
            }

            // If the creature is not already at destiny, the path is set
            notAtDestiny = creature.x != waterPosition.x || creature.y != waterPosition.y || creature.creatureLayer != 0;
            if (notAtDestiny)
                creature.SetPath(waterPosition.x, waterPosition.y);
        }

        private Vector2Int SectorToPosition(Vector2Int pos, int sector)
        {
            switch (sector)
            {
                case 0:
                    return new Vector2Int(pos.x + 1, pos.y);
                case 1:
                    return new Vector2Int(pos.x + 1, pos.y + 1);
                case 2:
                    return new Vector2Int(pos.x, pos.y + 1);
                case 3:
                    return new Vector2Int(pos.x - 1, pos.y + 1);
                case 4:
                    return new Vector2Int(pos.x - 1, pos.y);
                case 5:
                    return new Vector2Int(pos.x - 1, pos.y - 1);
                case 6:
                    return new Vector2Int(pos.x, pos.y - 1);
                case 7:
                    return new Vector2Int(pos.x + 1, pos.y - 1);
                default:
                    throw new Exception("Error calculating closest position to water in GoToDrink");
            }
        }
    }
}
