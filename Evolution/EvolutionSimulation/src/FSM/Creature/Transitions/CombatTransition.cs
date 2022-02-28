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
            threshold = 0.25f - (creature.stats.Aggressiveness / 100f); // TODO: A mayor agresividad mas se arriesga, revisar cifras
        }

        public override bool Evaluate()
        {
            return (creature.GetClosestCreatureReachable() != null || creature.hasBeenHit) 
                && creature.stats.Aggressiveness > 0    // TODO: Poner un valor de verdad en agresividad
                && creature.stats.CurrHealth >= creature.stats.MaxHealth * threshold;    // So it does not immediately return to combat while fleeing
            //TODO: revisar entidades vistas y considerar agresividad
        }

        public override string ToString()
        {
            return "CombatTransition";
        }

    }
}
