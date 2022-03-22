using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// Action that a creature eats an edible plant or a corpse depending on
    /// the creature's diet gaining some energy
    /// </summary>
    class Eating : CreatureState
    {
        int foodID;
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
            if(creature.IsCarnivorous())
            {
                EatCorpse();
            }
            else if(creature.IsHerbivorous())
            {
                EatPlant();
                creature.SafeEdiblePlant();
            }
            else//Omnivore
            {
                if (creature.Corpse())
                    EatCorpse();
                else if (creature.Plant())
                {
                    EatPlant();
                    creature.SafeEdiblePlant();
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
            creature.Plant(out foodID, out _);
            Entities.EdiblePlant closest = creature.world.GetStaticEntity(foodID) as Entities.EdiblePlant;
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
            creature.Corpse(out foodID, out _);
            closest = creature.world.GetStaticEntity(foodID) as Entities.Corpse;
                
            closest.ReceiveInteraction(creature, Entities.Interactions.eat);
            Console.WriteLine(creature.speciesName + " EATS CORPSE AT (" + closest.x + ", " + closest.y + ")");
        }
    }
}
