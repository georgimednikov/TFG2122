using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class WakeTransition : CreatureTransition
    {
        public WakeTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return creature.stats.CurrRest >= creature.stats.MaxRest;
        }

        public override string ToString()
        {
            return "WakeTransition";
        }

    }
}
