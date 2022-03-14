using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    class GoToEat : CreatureState
    {
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
            Vector3 nextPos = creature.GetNextPosOnPath();
            if (nextPos.X != -1 || nextPos.Y != -1 || nextPos.Z != -1)
                creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
            SetPath();
            Console.WriteLine(creature.speciesName + " GOES TO EAT (" + creature.x + ", " + creature.y + ")");
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

            Vector2Int foodPos;

            //Herbivore goes to a fruit
            if (creature.stats.Diet == Genetics.Diet.Herbivore)
                foodPos = creature.GetFruitPosition();

            //If there is a fresh corpse, that will be the objective of a carnivorous/omnivorous creature.
            //Because of how the creature memory works, if the creature is a scavenger the closest corpse will be considered fresh,
            //no matter its state.
            //Therefore, if there is a corpse saved in GetClosestCorpsePosition it will either be fresh or the creature will be a
            //scavenger and will not care, and if there is not, the alternative is a rotten corpse.

            //Carnivore goes to a corpse
            else if (creature.stats.Diet == Genetics.Diet.Carnivore)
            {
                if (creature.GetClosestCorpsePosition() == null)
                    foodPos = creature.GetClosestCorpsePosition();
                else
                    foodPos = creature.GetClosestRottenCorpsePosition();
            }
            else //Omnivore
            {
                //If there is a fresh corpse and no plant, then it goes to the corpse.
                if (creature.GetClosestCorpsePosition() != null && creature.GetFruitPosition() == null)
                    foodPos = creature.GetClosestCorpsePosition();

                //If there is a plant and no fresh corpse, it goes to a plant.
                else if (creature.GetClosestCorpsePosition() == null && creature.GetFruitPosition() != null)
                    foodPos = creature.GetFruitPosition();

                //If there is a corpse and a plant, it goes to the closest one.
                else if (creature.GetClosestCorpsePosition() != null && creature.GetFruitPosition() != null)
                {
                    // Goes to the closest food source
                    int distPlant = creature.DistanceToObjective(creature.GetFruitPosition()),
                        distCorpse = creature.DistanceToObjective(creature.GetClosestCorpsePosition());

                    if (distPlant < distCorpse)
                        foodPos = creature.GetFruitPosition();

                    else
                        foodPos = creature.GetClosestCorpsePosition();
                }
                //If the state machine got here the creature must be desperate and there has to be an alternate food source -> rotten corpse.
                else
                    foodPos = creature.GetClosestRottenCorpsePosition();
            }
            creature.SetPath(foodPos.x, foodPos.y);
        }
    }
}
