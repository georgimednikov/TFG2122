using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature is close to the drink objective
    /// </summary>
    class DrinkingTransition : CreatureTransition
    {
        public DrinkingTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            //TODO que el objetivo sea el de beber
            return creature.objective != null
               && Math.Abs(creature.objective.x - creature.x) < 1
               && Math.Abs(creature.objective.y - creature.y) < 1;
        }

        public override string ToString()
        {
            return "DrinkingTransition";
        }

    }
}
