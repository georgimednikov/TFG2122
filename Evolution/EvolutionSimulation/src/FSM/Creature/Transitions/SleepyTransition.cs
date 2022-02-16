using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class SleepyTransition : CreatureTransition
    {
        public SleepyTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return creature.stats.CurrRest <= 0.1 * creature.stats.MaxRest;
        }

        public override string ToString()
        {
            return "SleepyTransition";
        }
    }
}
