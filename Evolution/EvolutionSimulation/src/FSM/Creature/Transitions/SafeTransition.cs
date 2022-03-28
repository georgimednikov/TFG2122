using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class SafeTransition : CreatureTransition
    {
        public SafeTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return !creature.Enemy() && !creature.Menace();
        }

        public override string ToString()
        {
            return "SafeTransition";
        }

    }
}
