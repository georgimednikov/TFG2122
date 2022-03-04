﻿using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class EscapeTransition : CreatureTransition
    {
        /// Health threshold below which the creature will flee
        private float threshold;

        public EscapeTransition(Entities.Creature creature)
        {
            this.creature = creature;
            threshold = 0.25f - (creature.stats.Aggressiveness / 50f); // TODO: Magical numberinos
        }

        public override bool Evaluate()
        {
            return (creature.GetClosestCreature() != null || creature.hasBeenHit)
                && (creature.stats.Aggressiveness < 0.5 * (float)creature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Aggressiveness) ||  // Inferior to 50% equals non-aggresive
                creature.stats.CurrHealth < creature.stats.MaxHealth * threshold)   // So even an aggresive creature has self-preservation instincts
                && !creature.cornered;
            //TODO: revisar entidades vistas y considerar agresividad
        }

        public override string ToString()
        {
            return "EscapeTransition";
        }

    }
}
