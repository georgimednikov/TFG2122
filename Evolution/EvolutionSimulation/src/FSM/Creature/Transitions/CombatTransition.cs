﻿using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class CombatTransition : CreatureTransition
    {
        /// Health threshold above which the creature will fight
        private float threshold;

        public CombatTransition(Entities.Creature creature)
        {
            this.creature = creature;
            threshold = 0.25f - (creature.stats.Aggressiveness / UniverseParametersManager.parameters.combatTransitionHealthThresholdMultiplier); // TODO: A mayor agresividad mas se arriesga, revisar cifras
        }

        public override bool Evaluate()
        {
            return (creature.GetClosestCreatureReachablePosition() != null || creature.hasBeenHit) 
                && ((creature.stats.Aggressiveness >= creature.GetDanger()  // TODO: ajustar valor
                && creature.stats.CurrHealth >= creature.stats.MaxHealth * threshold)   // So it does not immediately return to combat while fleeing
                || creature.cornered);    // So it fights as a last resort when fleeing
        }

        public override string ToString()
        {
            return "CombatTransition";
        }

    }
}
