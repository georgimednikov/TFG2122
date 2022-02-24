using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                return creature.nearestPlant != null
                  && Math.Abs(creature.nearestPlant.x - creature.x) < 1
                  && Math.Abs(creature.nearestPlant.y - creature.y) < 1;
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
                || creature.nearestPlant != null
                && Math.Abs(creature.nearestPlant.x - creature.x) < 1
                && Math.Abs(creature.nearestPlant.y - creature.y) < 1; ;
            
        }

        public override string ToString()
        {
            return "EatingTransition";
        }

    }
}
