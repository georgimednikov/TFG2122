using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class CombatTransition : CreatureTransition
    {
        /// Health threshold above which the creature will fight
        private float threshold;

        public CombatTransition(Entities.Creature creature)
        {
            this.creature = creature;
            threshold = 0.25f - (creature.stats.Aggressiveness / 50f); // TODO: A mayor agresividad mas se arriesga, revisar cifras
        }

        public override bool Evaluate()
        {
            return (creature.GetClosestCreatureReachable() != null || creature.hasBeenHit) 
                && ((creature.stats.Aggressiveness >= 0.5 * (float)creature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Aggressiveness) 
                && creature.stats.CurrHealth >= creature.stats.MaxHealth * threshold)   // Under normal circunstances it will have self preservation isntincts
                || creature.cornered);    // So it fights as a last resort when fleeing
            //TODO: revisar entidades vistas y considerar agresividad
        }

        public override string ToString()
        {
            return "CombatTransition";
        }

    }
}
