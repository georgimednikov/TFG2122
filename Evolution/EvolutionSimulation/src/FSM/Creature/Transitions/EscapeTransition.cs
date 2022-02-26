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
            threshold = 0.25f; // TODO: Esto debe depender del cromosoma o algo
        }

        public override bool Evaluate()
        {
            return (creature.nearestEnemy != null || creature.hasBeenHit)
                && (creature.stats.Aggressiveness < 1 || 
                creature.stats.CurrHealth < creature.stats.MaxHealth * threshold);
            //TODO: revisar entidades vistas y considerar agresividad
        }

        public override string ToString()
        {
            return "EscapeTransition";
        }

    }
}
