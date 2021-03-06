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
            // The more aggresive a creature is, the lesser its danger range

            danger = (int)((UniverseParametersManager.parameters.fleeingTransitionMultiplier + 1) - creature.stats.Aggressiveness /
                (float)creature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Aggressiveness) * UniverseParametersManager.parameters.fleeingTransitionMultiplier);  
        }                                                                                                                                                               

        public override bool Evaluate()
        {
            Vector3Int obj; creature.Menace(out _, out obj);
            int deltaX = obj.x - creature.x,       // Direction of objective
                deltaY = obj.y - creature.y;

            return Math.Abs(deltaX) <= danger && Math.Abs(deltaY) <= danger;  // The danger zone is a square and the creature is within it
        }

        public override string ToString()
        {
            return "FleeTransition";
        }

    }
}
