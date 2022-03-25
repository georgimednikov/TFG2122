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
            return  creature.stats.CurrHealth <= 0
                || creature.stats.CurrAge >= creature.stats.LifeSpan;    
        }                                                                  

        public override string ToString()
        {
            return "DieTransition";
        }

    }
}
