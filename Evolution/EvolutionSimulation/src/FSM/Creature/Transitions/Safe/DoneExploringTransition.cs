using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if a creature needs to find a safe and does not know where to go
    /// Wander -> Explore
    /// </summary>
    class DoneExploringTransition : CreatureTransition
    {
        public DoneExploringTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            if (creature.wantMate && creature.GetClosestPossibleMatePosition() == null) return false;
            if (creature.IsThirsty() && creature.GetClosestWaterPosition() == null) return false;
            if (creature.IsTired() && creature.GetClosestSafePlacePosition() == null) return false;
            if (creature.IsHungry() && !creature.HasEatingObjective()) return false;
            if (creature.IsVeryHungry() && !creature.CanEatRottenCorpse()) return false;
            return true;
        }

        public override string ToString()
        {
            return "MateExploreTransition";
        }

    }
}
