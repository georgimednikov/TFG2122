using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class Revives : CreatureTransition, ITransition
    {
        public Revives(EvolutionSimulation.Creature creature)
        {
            this.creature = creature;
        }
        public bool Evaluate()
        {
            return creature.health > 0;
        }
    }
}
