using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class ThirstyTransition : CreatureTransition
    {
        public ThirstyTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return creature.stats.CurrHydration < 0.1 * creature.stats.MaxHydration;
        }

        public override string ToString()
        {
            return "ThirstyTransition";
        }

    }
}
