using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    class GoToDrink : CreatureState
    {
        Vector2Int ogWaterPos = new Vector2Int(-1, -1);
        Vector2Int shorePosition = new Vector2Int(-1, -1);
        bool notAtDestiny;
        public GoToDrink(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return creature.GetNextCostOnPath();
        }

        public override void OnEntry()
        {
            FindShore();
        }

        public override void Action()
        {
            Vector2Int waterpos = creature.WaterPosition();
            if (shorePosition == creature.WaterPosition() || ogWaterPos != creature.WaterPosition())
                FindShore();
            if (Math.Abs(creature.WaterPosition().x - shorePosition.x) > 1 || Math.Abs(creature.WaterPosition().y - shorePosition.y) > 1)
                Console.WriteLine("Posicion incorrecta para beber");
            if (notAtDestiny)
            {
                Vector3 nextPos = creature.GetNextPosOnPath();
               
                if (nextPos.X != -1 && nextPos.Y != -1) // TODO: no haria falta creo
                    creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
                if (nextPos.X == -1)
                {
                    creature.SetPath(shorePosition.x, shorePosition.y);
                }

                notAtDestiny = creature.x != shorePosition.x || creature.y != shorePosition.y || creature.creatureLayer != 0;
            }
        }
        public override string GetInfo()
        {
            return creature.speciesName + " with ID: " + creature.ID + " IN (" + creature.x + ", " + creature.y + ", " + creature.creatureLayer + ")" + " GOES TO DRINK AT (" + shorePosition.x + ", " + shorePosition.y + ")";
        }

        public override string ToString()
        {
            return "GoToDrinkState";
        }

        private void FindShore()
        {
            ogWaterPos.x = creature.WaterPosition().x; ogWaterPos.y = creature.WaterPosition().y;
            shorePosition.x = ogWaterPos.x; shorePosition.y = ogWaterPos.y;


            //Angle between the creatures position and the position of the water from 0 to 180 positive or negative.
            double degrees = Math.Atan2(shorePosition.y - creature.y, shorePosition.x - creature.x) * (180 / Math.PI);

            //Now the sector has to be accounted for.

            //If the resulting angle is negative it is converted to a degree from 0 to 360
            if (degrees < 0) degrees += 360;

            //There are 8 sectors representing the 8 tiles that surround another, each one with an angle of 360/8 = 45 degrees.
            //Half a quadrant of offset to start counting the angle each sector represents from 0.
            degrees += (45.0 / 2) % 360;

            int sector = (int)(degrees / 45);
            int cont = 0;
            while (!creature.world.checkBounds(shorePosition.x, shorePosition.y) ||
                creature.world.map[shorePosition.x, shorePosition.y].isWater)
            {
                //The increment has to have the same sign as cont to add their values without possible substractions,
                //but cont's sign has to be mantained to alternate between going "left" or "right" realtive to the current sector.
                int inc = 1; if (cont < 0) inc *= -1;
                cont = (cont + inc) * -1;
                sector = (sector + cont) % 8;
                if (sector < 0) sector += 8;
                shorePosition = SectorToPosition(ogWaterPos, sector);
            }

            // If the creature is not already at destiny, the path is set
            notAtDestiny = creature.x != shorePosition.x || creature.y != shorePosition.y || creature.creatureLayer != 0;
            if (Math.Abs(creature.WaterPosition().x - shorePosition.x) > 1 || Math.Abs(creature.WaterPosition().y - shorePosition.y) > 1)
                Console.WriteLine("Posicion incorrecta para beber");
            if (notAtDestiny)
                creature.SetPath(shorePosition.x, shorePosition.y);
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
