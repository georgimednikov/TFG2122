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
    class TryMateTransition : CreatureTransition
    {
        public TryMateTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            //TODO que el objetivo sea el de hacer mate
            return creature.objective != null
               && Math.Abs(creature.objective.x - creature.x) < 1
               && Math.Abs(creature.objective.y - creature.y) < 1;
        }

        public override string ToString()
        {
            return "TryMateTransition";
        }

    }
}
