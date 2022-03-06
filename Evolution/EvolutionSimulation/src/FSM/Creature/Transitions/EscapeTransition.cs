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
            return (creature.GetClosestCreature() != null || creature.hasBeenHit)
                && (creature.stats.Aggressiveness < UniverseParametersManager.parameters.escapeTransitionAggressivenessThreshold * 
                (float)creature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Aggressiveness) ||  // Inferior to 50% equals non-aggresive
                creature.stats.CurrHealth < creature.stats.MaxHealth * threshold)   // So even an aggresive creature has self-preservation instincts
                && !creature.cornered;
            //TODO: revisar entidades vistas y considerar agresividad

            //return (creature.GetClosestCreatureReachablePosition() != null || creature.hasBeenHit) &&
            //    creature.stats.Aggressiveness > creature.GetDanger() &&
            //    creature.stats.CurrHealth >= creature.stats.MaxHealth * threshold;    // So it does not immediately return to combat while fleeing
            
            //return (creature.GetClosestCreaturePosition() != null || creature.hasBeenHit)
            //    && (creature.stats.Aggressiveness < UniverseParametersManager.parameters.escapeTransitionAggressivenessThreshold * (float)creature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Aggressiveness) ||  // Inferior to 50% equals non-aggresive
            //    creature.stats.CurrHealth < creature.stats.MaxHealth * threshold);  // So even an aggresive creature has self-preservation instincts
            //TODO: considerar danger   
        }

        public override string ToString()
        {
            return "EscapeTransition";
        }

    }
}
