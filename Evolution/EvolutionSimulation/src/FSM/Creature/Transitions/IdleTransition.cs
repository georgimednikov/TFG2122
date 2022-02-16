using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class IdleTransition : CreatureTransition
    {
        public IdleTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return false;
            //TODO: hacer bien esta transicion
        }
    
    }
}
