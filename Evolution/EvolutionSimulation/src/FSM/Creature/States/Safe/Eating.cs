using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// Action that a creature eats an edible plant or a corpse depending on
    /// the creature's diet gaining some energy
    /// </summary>
    class Eating : CreatureState
    {
        public Eating(Entities.Creature c) : base(c) { creature = c; }

        public override void OnEntry()
        {
            creature.stats.ActionPerceptionPercentage = UniverseParametersManager.parameters.actionPerceptionPercentage;
        }

        public override int GetCost()
        {
            return UniverseParametersManager.parameters.eatingCostMultiplier * creature.stats.Metabolism;
        }

        public override void Action()
        {
            if(creature.stats.Diet == Genetics.Diet.Carnivore )
            {
                EatCorpse();
            }
            else if(creature.stats.Diet == Genetics.Diet.Herbivore)
            {
                EatPlant();
                creature.SafePlantFound();
            }
            else//Omnivore
            {
                if (creature.GetClosestCorpse() != null)
                    EatCorpse();
                else if (creature.GetFruit() != null)
                {
                    EatPlant();
                    creature.SafePlantFound();
                }
            }
        }

        public override void OnExit()
        {
            creature.stats.ActionPerceptionPercentage = 1;
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
            Entities.EdiblePlant closest = creature.GetFruit();
            creature.stats.CurrEnergy += closest.Eat();
            Console.WriteLine(creature.speciesName + " EATS FRUIT AT (" + closest.x + ", " + closest.y + ")");
        }

        /// <summary>
        /// Eat the nearest corpse, gaining energy and letting the corpse know
        /// that it has been eaten
        /// </summary>
        protected void EatCorpse()
        {
            Entities.Corpse closest;
            if (creature.GetClosestCorpse() != null)
                closest = creature.GetClosestCorpse();
            else
                closest = creature.GetClosestRottenCorpse();
            closest.ReceiveInteraction(creature, Entities.Interactions.eat);
            Console.WriteLine(creature.speciesName + " EATS CORPSE AT (" + closest.x + ", " + closest.y + ")");
        }
    }
}
