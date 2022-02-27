﻿namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the nearestMate (female) doesn't want to mate
    /// TryToMate -> Wander
    /// </summary>
    class StopTryMateTransition : CreatureTransition
    {
        public StopTryMateTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            if (creature.stats.Gender == Genetics.Gender.Female)//just in case
                return true;

            return creature.nearestMate == null || (!creature.nearestMate.wantMate && !creature.mating)
                || creature.IsHungry() || creature.IsThirsty() || creature.IsTired();
        }

        public override string ToString()
        {
            return "StopTryMateTransition";
        }

    }
}