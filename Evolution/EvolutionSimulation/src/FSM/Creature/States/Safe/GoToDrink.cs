using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    class GoToDrink : CreatureState
    {
        Vector2Int finalPosition;
        public GoToDrink(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return creature.GetNextCostOnPath();
        }

        public override void OnEntry()
        {
            Vector2Int posToDrink = creature.WaterPosition();

            //Angle between the creatures position and the position of the water from 0 to 180 positive or negative.
            double degrees = Math.Atan2(creature.y - posToDrink.y, creature.x - posToDrink.x) * (180 / Math.PI);

            //Now the sector has to be accounted for.

            //If the resulting angle is negative it is converted to a degree from 0 to 360
            if (degrees < 0) degrees += 360;

            //There are 8 sectors representing the 8 tiles that surround another, each one with an angle of 360/8 = 45 degrees.
            //Half a quadrant of offset to start counting the angle each sector represents from 0.
            degrees += 45.0 / 2;

            int sector = (int)(degrees / 45) % 8;
            finalPosition = SectorToPosition(posToDrink, sector);
            creature.SetPath(finalPosition.x, finalPosition.y);
        }

        public override void Action()
        {
            Vector3 nextPos = creature.GetNextPosOnPath();
            if (nextPos.X < 0) return; //If it is already in the right spot it should not move.
            creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
        }
        public override string GetInfo()
        {
            return creature.speciesName + " with ID: " + creature.ID + " IN (" + creature.x + ", " + creature.y + ")" + " GOES TO DRINK AT (" + finalPosition.x + ", " + finalPosition.y + ")";
        }

        public override string ToString()
        {
            return "GoToDrinkState";
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
