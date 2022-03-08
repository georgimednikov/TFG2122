using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    class GoToEat : CreatureState
    {
        Tuple<int, int> foodPos;

        public GoToEat(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return creature.GetNextCostOnPath();
        }

        public override void OnEntry()
        {
            SetPath();
        }

        public override void Action()
        {
            Console.WriteLine("GoToEat action");
            Vector3 nextPos = creature.GetNextPosOnPath();
            if (nextPos.X != -1 || nextPos.Y != -1 || nextPos.Z != -1)
                creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
            SetPath();
        }

        public override string ToString()
        {
            return "GoToEatState";
        }

        /// <summary>
        /// Set the path of the creature depending on his diet and the closest 
        /// food that he knows
        /// </summary>
        private void SetPath()
        {
            //TODO no deberia pasar esto porque saldria del estado por las transiciones,
            //lo dejamos por si acaso? programacion defensiva. Si lo dejamos, hay que hacerlo en otros estados
            //if (creature.GetClosestFruit() == null && creature.GetClosestCorpse() == null)
            //    return;

            //The position of the fruit in which the creature would be most interested in is decided
            //between the closest one and the closest one that has proven to be safe, based on distance.
            Tuple<int, int> fruitPos;
            if (ClosestOverSafeFruit())
                fruitPos = creature.GetClosestFruitPosition();
            else
                fruitPos = creature.GetSafeFruitPosition();

            //Carnivore goes to a corpse
            if (creature.stats.Diet == Genetics.Diet.Carnivore)
            {
                foodPos = creature.GetClosestCorpsePosition();
                creature.SetPath(foodPos.Item1, foodPos.Item2);
            }
            //Herbivore goes to a fruit
            else if (creature.stats.Diet == Genetics.Diet.Herbivore)
            {
                foodPos = fruitPos;
                creature.SetPath(foodPos.Item1, foodPos.Item2);
            }
            else //Omnivore
            {
                if (creature.GetClosestCorpsePosition() == null)
                {
                    foodPos = fruitPos;
                    creature.SetPath(foodPos.Item1, foodPos.Item2);
                }
                else if (fruitPos == null)
                {
                    foodPos = creature.GetClosestCorpsePosition();
                    creature.SetPath(foodPos.Item1, foodPos.Item2);
                }
                else
                {
                    // Goes to the closest food (nearestEdiblePlant or Corpse)
                    int distPlant = creature.DistanceToObjective(fruitPos),
                        distCorpse = creature.DistanceToObjective(creature.GetClosestCorpse());
                    if (distPlant < distCorpse)
                    {
                        foodPos = fruitPos;
                        creature.SetPath(foodPos.Item1, foodPos.Item2);
                    }
                    else
                    {
                        foodPos = creature.GetClosestCorpsePosition();
                        creature.SetPath(foodPos.Item1, foodPos.Item2);
                    }
                }
            }
        }

        private bool ClosestOverSafeFruit()
        {
            return creature.GetSafeFruitPosition() == null ||
                (creature.DistanceToObjective(creature.GetSafeFruitPosition()) >
                 creature.DistanceToObjective(creature.GetClosestFruitPosition()) * UniverseParametersManager.parameters.safePrefferedOverClosestResourceRatio);
        }
    }
}
