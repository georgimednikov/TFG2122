using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class HungerTransition : CreatureTransition
    {
        public HungerTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return false;
            //TODO: return creature.energy < 5;
        }
    
    }
}
