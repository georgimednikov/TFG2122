using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class AttackTransition : CreatureTransition
    {
        public AttackTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            Vector2Int obj; creature.Enemy(out _, out obj);
            int deltaX = obj.x - creature.x,       // Direction of objective
                deltaY = obj.y - creature.y;

            return Math.Abs(deltaX) <= UniverseParametersManager.parameters.adjacentLength && Math.Abs(deltaY) <= UniverseParametersManager.parameters.adjacentLength;  // This implies inisde melee range
        }

        public override string ToString()
        {
            return "AttackTransition";
        }
    }
}
