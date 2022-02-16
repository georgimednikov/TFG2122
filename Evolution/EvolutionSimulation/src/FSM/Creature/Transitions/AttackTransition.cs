using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class AttackTransition : CreatureTransition
    {
        public AttackTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return (creature.objective != null || creature.hasBeenHit) 
                && creature.stats.Aggressiveness > 0;
            //TODO: revisar entidades vistas y considerar agresividad
        }
    
    }
}
