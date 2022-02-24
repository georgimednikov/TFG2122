using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature is close to the eating objective
    /// </summary>
    class EatingTransition : CreatureTransition
    {
        public EatingTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            if(creature.stats.Diet == Genetics.Diet.Herbivore )
            {
                return creature.nearestEdiblePlant != null
                  && Math.Abs(creature.nearestEdiblePlant.x - creature.x) < 1
                  && Math.Abs(creature.nearestEdiblePlant.y - creature.y) < 1;
            }

            if (creature.stats.Diet == Genetics.Diet.Carnivore )
            {
                return creature.nearestCorpse != null
                  && Math.Abs(creature.nearestCorpse.x - creature.x) < 1
                  && Math.Abs(creature.nearestCorpse.y - creature.y) < 1;
            }

            //Omnivore
            return (creature.nearestCorpse != null
                && Math.Abs(creature.nearestCorpse.x - creature.x) < 1
                && Math.Abs(creature.nearestCorpse.y - creature.y) < 1) 
                || creature.nearestEdiblePlant != null
                && Math.Abs(creature.nearestEdiblePlant.x - creature.x) < 1
                && Math.Abs(creature.nearestEdiblePlant.y - creature.y) < 1; ;
            
        }

        public override string ToString()
        {
            return "EatingTransition";
        }

    }
}
