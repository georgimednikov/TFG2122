using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class Moves : CreatureTransition, ITransition
    {
        public Moves(EvolutionSimulation.Creature creature)
        {
            this.creature = creature;
        }
        public bool Evaluate()
        {
            return creature.Speed > 0;
        }
    }
}
