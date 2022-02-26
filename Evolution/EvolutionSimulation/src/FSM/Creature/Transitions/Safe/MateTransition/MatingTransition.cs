using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature is going to mate
    /// TryMate -> Mating for males
    /// a lot of states -> Mating for females
    /// </summary>
    class MatingTransition : CreatureTransition
    {
        public MatingTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return creature.mating;
        }

        public override string ToString()
        {
            return "MatingTransition";
        }
    }
}
