using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// Action that eat an edible plant or a corpse depending on the creature's diet
    /// gaining some energy
    /// </summary>
    class Eating : CreatureState
    {
        public Eating(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return 10 * creature.stats.Metabolism;
        }

        public override void Action()
        {
            Console.WriteLine("Eating action");
            if(creature.stats.Diet == Genetics.Diet.Carnivore )
            {
                EatCorpse();
            }
            else if(creature.stats.Diet == Genetics.Diet.Herbivore)
            {
                EatPlant();
            }
            else//Omnivore
            {
                if (creature.nearestCorpse == null)
                    EatPlant();
                else if (creature.nearestEdiblePlant == null)
                    EatCorpse();
                else
                {
                    //Eat the nearest food (nearestEdiblePlant or Corpse)
                    int distPlant = creature.DistanceToObjective(creature.nearestEdiblePlant),
                        distCorpse = creature.DistanceToObjective(creature.nearestCorpse);
                    if (distPlant < distCorpse)
                        EatPlant();
                    else
                        EatCorpse();
                }
            }
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
            creature.nearestCorpse.ReceiveInteraction(creature, Entities.Interactions.eat);
        }
    }
}
