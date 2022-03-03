using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class HideTransition : CreatureTransition
    {
        // Range at which the creature feels threatened
        private int danger;

        public HideTransition(Entities.Creature creature)
        {
            this.creature = creature;
            danger = (int)((UniverseParametersManager.parameters.hidingTransitionMultiplier + 1) - creature.stats.Aggressiveness /
                (float)creature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Aggressiveness) * UniverseParametersManager.parameters.hidingTransitionMultiplier);  // The more aggresive a creature is, the lesser its danger range
        }                                                                                                                                               // And therefore, the less it runs away

        public override bool Evaluate()
        {
            int oX = creature.GetClosestCreature().x,   // Objective's position
                oY = creature.GetClosestCreature().y;
            int deltaX = oX - creature.x,       // Direction of objective
                deltaY = oY - creature.y;

            return Math.Abs(deltaX) > danger || Math.Abs(deltaY) > danger;  // It is "out of danger"
        }

        public override string ToString()
        {
            return "HideTransition";
        }

    }
}
