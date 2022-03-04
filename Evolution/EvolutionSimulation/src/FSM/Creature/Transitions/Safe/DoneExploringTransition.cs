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
            if (creature.wantMate && creature.GetClosestPossibleMate() == null) return false;
            if (creature.IsThirsty() && creature.GetClosestWater() == null) return false;
            if (creature.IsTired() && creature.GetClosestSafePlace() == null) return false;
            if (creature.IsHungry() && !creature.HasEatingObjective()) return false;
            return true;
        }

        public override string ToString()
        {
            return "MateExploreTransition";
        }

    }
}
