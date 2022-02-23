using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class CombatTransition : CreatureTransition
    {
        public CombatTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return (creature.objective != null || creature.hasBeenHit) 
                && creature.stats.Aggressiveness > 0;
            //TODO: revisar entidades vistas y considerar agresividad
        }

        public override string ToString()
        {
            return "CombatTransition";
        }

    }
}
