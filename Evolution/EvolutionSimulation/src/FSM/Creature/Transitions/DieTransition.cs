using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class DieTransition : CreatureTransition
    {
        public DieTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return  creature.stats.currHealth <= 0
                || creature.stats.currAge++ >= creature.stats.lifeSpan;
        }
    
    }
}
