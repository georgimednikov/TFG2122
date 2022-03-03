using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class FleeTransition : CreatureTransition
    {
        // Range at which the creature feels threatened
        private int danger;

        public FleeTransition(Entities.Creature creature)
        {
            this.creature = creature;
            danger = (int)((UniverseParametersManager.parameters.fleeingTransitionMultiplier + 1) - creature.stats.Aggressiveness /
                (float)creature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Aggressiveness) * UniverseParametersManager.parameters.fleeingTransitionMultiplier);  // The more aggresive a creature is, the lesser its danger range
        }                                                                                                                                                            // And therefore, the less it runs away

        public override bool Evaluate()
        {
            int oX = creature.GetClosestCreature().x,   // Objective's position
                oY = creature.GetClosestCreature().y;
            int deltaX = oX - creature.x,       // Direction of objective
                deltaY = oY - creature.y;

            return Math.Abs(deltaX) <= danger && Math.Abs(deltaY) <= danger;  // The danger zone is a square and the creature is within it
        }

        public override string ToString()
        {
            return "FleeTransition";
        }

    }
}
