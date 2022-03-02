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

        public override void Action()
        {
            SetPath();
            Console.WriteLine("GoToEat action");
            Vector3 nextPos = creature.GetNextPosOnPath();
            creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
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
            //Carnivore go to a corpse
            if (creature.stats.Diet == Genetics.Diet.Carnivore)
            {
                creature.SetPath(creature.GetClosestCorpse().x, creature.GetClosestCorpse().y);
            }//Herbivore go to a fruit
            else if (creature.stats.Diet == Genetics.Diet.Herbivore)
            {
                creature.SetPath(creature.GetClosestFruit().x, creature.GetClosestFruit().y);
            }
            else//Omnivore
            {
                if (creature.GetClosestCorpse() == null)
                    creature.SetPath(creature.GetClosestFruit().x, creature.GetClosestFruit().y);
                else if (creature.GetClosestFruit() == null)
                    creature.SetPath(creature.GetClosestCorpse().x, creature.GetClosestCorpse().y);
                else
                {
                    // Go to the closest food (nearestEdiblePlant or Corpse)
                    int distPlant = creature.DistanceToObjective(creature.GetClosestFruit()),
                        distCorpse = creature.DistanceToObjective(creature.GetClosestCorpse());
                    if (distPlant < distCorpse)
                        creature.SetPath(creature.GetClosestFruit().x, creature.GetClosestFruit().y);
                    else
                        creature.SetPath(creature.GetClosestCorpse().x, creature.GetClosestCorpse().y);
                }
            }
        }
    }
}
