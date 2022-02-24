using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// Action that eat a edible plant or a corpse depending on the creature's diet
    /// gaining some energy
    /// </summary>
    class Eating : CreatureState
    {
        public Eating(Entities.Creature c) : base(c) { creature = c; }

        public override bool canPerformAction(int actionPoints)
        {
            return actionPoints < 1000;
        }

        public override int Action()
        {
            Console.WriteLine("Eating action");
            if(creature.stats.Diet == Genetics.Diet.Carnivore )
            {
                //if (creature.nearestCorpse != null)//TODO hace falta esto? creo que no por las transiciones
                EatCorpse();
            }
            else if(creature.stats.Diet == Genetics.Diet.Herbivore)
            {
                //if (creature.nearestEdiblePlant != null)//TODO hace falta esto? creo que no por las transiciones
                EatPlant();
            }
            else
            {
                if (creature.nearestCorpse == null)
                    EatPlant();
                else if (creature.nearestEdiblePlant == null)
                    EatCorpse();
                else
                {
                    //Eat the nearest food (nearestEdiblePlant or Corpse)
                    int x1, y1, x2, y2;
                    x1 = Math.Abs(creature.x - creature.nearestEdiblePlant.x);
                    y1 = Math.Abs(creature.y - creature.nearestEdiblePlant.y);
                    x2 = Math.Abs(creature.x - creature.nearestCorpse.x);
                    y2 = Math.Abs(creature.y - creature.nearestCorpse.y);
                    int dist1 = (int)Math.Sqrt(Math.Pow(x1, 2) + Math.Pow(y1, 2)),
                        dist2 = (int)Math.Sqrt(Math.Pow(x2, 2) + Math.Pow(y2, 2));
                    if (dist1 < dist2)
                        EatPlant();
                    else
                        EatCorpse();
                }
            }
            return 10 * creature.stats.Metabolism; // Cost of the action performed
        }

        public override string ToString()
        {
            return "EatingState";
        }

        /// <summary>
        /// Eat the nearest plant, gaining energy and letting the plant know
        /// that it has been eaten
        /// </summary>
        protected void EatPlant()
        {
            creature.stats.CurrEnergy += creature.nearestEdiblePlant.Eat();
        }

        /// <summary>
        /// Eat the nearest corpse, gaining energy and letting the corpse know
        /// that it has been eaten
        /// </summary>
        protected void EatCorpse()
        {
            //TODO
            //creature.AddInteraction(Entities.Interactions.eat, creature.nearestCorpse);
        }
    }
}
