using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature is close to the mate objective. This is only for males
    /// GoToMate -> TryMate
    /// </summary>
    class TryMateTransition : CreatureTransition
    {
        public TryMateTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            Vector3Int pos; 
                 
            return creature.Mate(out _, out pos)
                && Math.Abs(pos.x - creature.x) < UniverseParametersManager.parameters.adjacentLength
                && Math.Abs(pos.y - creature.y) < UniverseParametersManager.parameters.adjacentLength
                && pos.z == (int)creature.creatureLayer;
        }

        public override string ToString()
        {
            return "TryMateTransition";
        }

    }
}
