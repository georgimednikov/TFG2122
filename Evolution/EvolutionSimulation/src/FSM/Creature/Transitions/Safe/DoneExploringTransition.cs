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
            if (!creature.CheckTemperature(creature.x, creature.y))
                if (creature.SafeTemperaturePosition() == null) return false;
                else return true;

            if (creature.IsTired())
                if (creature.SafePosition() == null)
                    return false;
                else return true;

            if (creature.IsThirsty())
                if (creature.WaterPosition() == null) return false;
                else return true;

            if (creature.IsHungry())
                if (!creature.HasEatingObjective()) return false;
                else return true;

            if (creature.stats.Gender == Genetics.Gender.Male
                && !creature.Mate()) return false;
            return true;
        }

        public override string ToString()
        {
            return "DoneExploringTransition";
        }

    }
}
