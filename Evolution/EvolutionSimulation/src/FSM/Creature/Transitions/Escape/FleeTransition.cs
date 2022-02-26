using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class FleeTransition : CreatureTransition
    {
        // Range at which the creature feels threatened
        private int danger;

        public FleeTransition(Entities.Creature creature)
        {
            this.creature = creature;
            danger = 5;    // TODO: Esto debe depender del cromosoma o algo
        }

        public override bool Evaluate()
        {
            int oX = creature.nearestEnemy.x,   // Objective's position
                oY = creature.nearestEnemy.y;
            int deltaX = oX - creature.x,       // Direction of objective
                deltaY = oY - creature.y;

            return deltaX <= danger || deltaY <= danger;  // The danger zone is a square and the creature is within it
        }

        public override string ToString()
        {
            return "AttackTransition";
        }

    }
}
