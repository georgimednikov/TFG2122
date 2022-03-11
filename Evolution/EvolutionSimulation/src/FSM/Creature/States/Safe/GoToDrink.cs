using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    class GoToDrink : CreatureState
    {
        Vector2Int posToDrink;
        public GoToDrink(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return creature.GetNextCostOnPath();
        }

        public override void OnEntry()
        {
            //The position of the water mass in which the creature would be most interested in is decided
            //between the closest one and the closest one that has proven to be safe, based on distance.
            //Tuple<int, int> posToDrink;
            if (creature.GetSafeWaterPosition() == null || 
                (creature.DistanceToObjective(creature.GetSafeWaterPosition()) >
                creature.DistanceToObjective(creature.GetClosestWaterPosition()) * UniverseParametersManager.parameters.safePrefferedOverClosestResourceRatio))
                posToDrink = creature.GetClosestWaterPosition();
            else
                posToDrink = creature.GetSafeWaterPosition();

            //Angle between the creatures position and the position of the water from 0 to 180 positive or negative.
                double degrees = Math.Atan2(creature.y - posToDrink.x, creature.x - posToDrink.y) * (180 / Math.PI);

            //Now the sector has to be accounted for.

            //If the resulting angle is negative it is converted to a degree from 0 to 360
            if (degrees < 0) degrees += 360;

            //There are 8 sectors representing the 8 tiles that surround another, each one with an angle of 360/8 = 45 degrees.
            //Half a quadrant of offset to start counting the angle each sector represents from 0.
            degrees += 45.0 / 2;

            int sector = (int)(degrees / 45) % 8;
            Tuple<int, int> finalPosition = SectorToPosition(posToDrink, sector);
            //int cont = 0;
            //while (creature.world.map[finalPosition.Item1, finalPosition.Item2].isWater)
            //{
            //    //The increment has to have the same sign as cont to add their values without possible substractions,
            //    //but cont's sign has to be mantained to alternate between going "left" or "right" realtive to the current sector.
            //    int inc = 1; if (cont < 0) inc *= -1;
            //    cont = (cont + inc) * -1;
            //    sector = (sector + cont) % 8;
            //    finalPosition = SectorToPosition(sector);
            //}

            creature.SetPath(finalPosition.Item1, finalPosition.Item2);
        }

        public override void Action()
        {
            Console.WriteLine("Go to water action");
            Vector3 nextPos = creature.GetNextPosOnPath();
            if (nextPos.X < 0) return; //If it is already in the right spot it should not move.
            creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
        }

        public override string ToString()
        {
            return "GoToDrinkState";
        }

        private Tuple<int, int> SectorToPosition(Vector2Int pos, int sector)
        {
            switch (sector)
            {
                case 0:
                    return new Tuple<int, int>(pos.x + 1, pos.y);
                case 1:
                    return new Tuple<int, int>(pos.x + 1, pos.y + 1);
                case 2:
                    return new Tuple<int, int>(pos.x, pos.y + 1);
                case 3:
                    return new Tuple<int, int>(pos.x - 1, pos.y + 1);
                case 4:
                    return new Tuple<int, int>(pos.x - 1, pos.y);
                case 5:
                    return new Tuple<int, int>(pos.x - 1, pos.y - 1);
                case 6:
                    return new Tuple<int, int>(pos.x, pos.y - 1);
                case 7:
                    return new Tuple<int, int>(pos.x + 1, pos.y - 1);
                default:
                    throw new Exception("Error calculating closest position to water in GoToDrink");
            }
        }
    }
}
