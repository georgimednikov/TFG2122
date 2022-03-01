using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature is close to the mate objective. This is only for males
    /// GoToMate -> TryMate
    /// </summary>
    class TryMateTransition : CreatureTransition
    {
        public TryMateTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            if (creature.stats.Gender == Genetics.Gender.Female)//just in case
                return false;

            return creature.GetClosestPossibleMate() != null
               && Math.Abs(creature.GetClosestPossibleMate().x - creature.x) < 1
               && Math.Abs(creature.GetClosestPossibleMate().y - creature.y) < 1;
        }

        public override string ToString()
        {
            return "TryMateTransition";
        }

    }
}
