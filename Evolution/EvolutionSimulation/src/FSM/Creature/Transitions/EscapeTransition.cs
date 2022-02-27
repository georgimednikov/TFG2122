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
            threshold = 0.25f - (creature.stats.Aggressiveness / 100f); // TODO: Mirar parametros mas a fondo
        }

        public override bool Evaluate()
        {
            return (creature.nearestEnemy != null || creature.hasBeenHit)
                && (creature.stats.Aggressiveness < 1 ||    // TODO: Poner un valor de verdad en agresividad
                creature.stats.CurrHealth < creature.stats.MaxHealth * threshold);  // So even an aggresive creature has self-preservation instincts
            //TODO: revisar entidades vistas y considerar agresividad
        }

        public override string ToString()
        {
            return "EscapeTransition";
        }

    }
}
