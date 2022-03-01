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
            // Do not go to a mate if the creature is a female
            if (creature.stats.Gender == Genetics.Gender.Female)
                return false;

            return creature.GetClosestPossibleMate() != null
                && creature.GetClosestPossibleMate().wantMate    //the objective wantMate (just in case)                                                                    
                && !creature.stats.IsNewBorn();     // and it has to be adult

        }

        public override string ToString()
        {
            return "MateTransition";
        }

    }
}
