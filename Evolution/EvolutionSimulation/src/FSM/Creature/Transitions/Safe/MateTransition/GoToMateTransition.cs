using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// This transition is for males creatures and is to go to a female
    /// who is in heat.
    /// Wander -> Go to mate
    /// </summary>
    class GoToMateTransition : CreatureTransition
    {
        public GoToMateTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            // Do not go to a mate if the creature is a female or a new born
            if (creature.stats.Gender == Genetics.Gender.Female || creature.stats.IsNewBorn())
                return false;

            return creature.Mate();

        }

        public override string ToString()
        {
            return "MateTransition";
        }

    }
}
