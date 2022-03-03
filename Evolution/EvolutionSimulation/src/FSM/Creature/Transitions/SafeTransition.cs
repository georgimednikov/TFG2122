using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class SafeTransition : CreatureTransition
    {
        public SafeTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return (creature.GetClosestCreature() == null && !creature.hasBeenHit) ||
                (creature.GetClosestCreature() != null && creature.GetClosestCreature().stats.Aggressiveness < UniverseParametersManager.parameters.safeTransitionAggressivenessThreshold);
            //TODO: revisar entidades vistas y considerar agresividad
        }

        public override string ToString()
        {
            return "SafeTransition";
        }

    }
}
