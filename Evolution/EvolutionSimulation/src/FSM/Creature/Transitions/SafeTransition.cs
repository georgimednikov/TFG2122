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
            return (creature.nearestEnemy == null && !creature.hasBeenHit) ||
                (creature.nearestEnemy != null && creature.nearestEnemy.stats.Aggressiveness < 1);
            //TODO: revisar entidades vistas y considerar agresividad
        }

        public override string ToString()
        {
            return "SafeTransition";
        }

    }
}
