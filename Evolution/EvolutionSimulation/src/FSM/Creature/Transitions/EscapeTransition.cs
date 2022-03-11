using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class EscapeTransition : CreatureTransition
    {
        /// Health threshold below which the creature will flee
        private float threshold;

        public EscapeTransition(Entities.Creature creature)
        {
            this.creature = creature;
            threshold = 0.25f - (creature.stats.Aggressiveness / UniverseParametersManager.parameters.escapeTransitionHealthThresholdMultiplier); // TODO: Magical numberinos
        }

        public override bool Evaluate()
        {
            return (creature.GetClosestCreaturePosition() != null || creature.hasBeenHit)
                && (creature.stats.Aggressiveness < creature.GetDanger() ||         // TODO: ajustar valores
                creature.stats.CurrHealth < creature.stats.MaxHealth * threshold)   // So even an aggresive creature has self-preservation instincts
                && !creature.cornered;
        }

        public override string ToString()
        {
            return "EscapeTransition";
        }

    }
}
