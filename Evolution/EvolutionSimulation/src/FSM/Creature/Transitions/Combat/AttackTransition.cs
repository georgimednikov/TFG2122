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
            int oX = creature.GetEnemy().x,   // Objective's position
                oY = creature.GetEnemy().y;
            int deltaX = oX - creature.x,       // Direction of objective
                deltaY = oY - creature.y;

            return Math.Abs(deltaX) <= UniverseParametersManager.parameters.adjacentLength && Math.Abs(deltaY) <= UniverseParametersManager.parameters.adjacentLength;  // This implies inisde melee range
        }

        public override string ToString()
        {
            return "AttackTransition";
        }

    }
}
