using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if a creature needs to find a safe and does not know where to go
    /// Explore -> Wander
    /// </summary>
    class DoneExploringTransition : CreatureTransition
    {
        public DoneExploringTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            if (creature.wantMate && !creature.Mate()) return false;
            if (creature.IsThirsty() && creature.WaterPosition() == null) return false;
            if (creature.IsTired() && creature.SafePosition() == null) return false;
            if (creature.IsHungry() && !creature.HasEatingObjective()) return false;
            return true;
        }

        public override string ToString()
        {
            return "DoneExploringTransition";
        }

    }
}
