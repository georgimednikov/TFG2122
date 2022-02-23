using System;

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
            return creature.stats.CurrEnergy < 0.1 * creature.stats.MaxEnergy ;
        }

        public override string ToString()
        {
            return "HungerTransition";
        }

    }
}
