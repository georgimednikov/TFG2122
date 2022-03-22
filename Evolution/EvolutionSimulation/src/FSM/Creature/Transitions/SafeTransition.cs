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
            int id; Vector2Int pos;
            return ((!creature.Menace(out id, out pos) || creature.DistanceToObjective(pos) > creature.stats.Perception) &&
                !creature.HasBeenAttacked()) ||
                (creature.Menace() && creature.world.GetCreature(id).stats.Aggressiveness < UniverseParametersManager.parameters.safeTransitionAggressivenessThreshold);
            //TODO: revisar entidades vistas y considerar agresividad
        }

        public override string ToString()
        {
            return "SafeTransition";
        }

    }
}
