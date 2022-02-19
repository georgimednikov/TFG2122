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
                || creature.stats.CurrAge++ >= creature.stats.LifeSpan;    // Adjustment needed since LifeSpan is apparently measured in years
        }                                                                  // TODO: No va a ser years ya te digo yo, es una burrada                                                                                             // TODO: ninguna practicamente llegue
                                                                           // TODO: No creo que ninguna llegue a tantisimo
        public override string ToString()
        {
            return "DieTransition";
        }

    }
}
