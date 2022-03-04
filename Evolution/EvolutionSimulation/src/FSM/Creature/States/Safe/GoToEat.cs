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

            //Carnivore go to a corpse
            if (creature.stats.Diet == Genetics.Diet.Carnivore && creature.GetClosestCorpse() != null)
            {
                creature.SetPath(creature.GetClosestCorpse().x, creature.GetClosestCorpse().y);
            }//Herbivore go to a fruit
            else if (creature.stats.Diet == Genetics.Diet.Herbivore && creature.GetClosestFruit() != null)
            {
                creature.SetPath(creature.GetClosestFruit().x, creature.GetClosestFruit().y);
            }
            else //Omnivore
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
