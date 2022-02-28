using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class HideTransition : CreatureTransition
    {
        // Range at which the creature feels threatened
        private int danger;

        public HideTransition(Entities.Creature creature)
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

            return Math.Abs(deltaX) > danger || Math.Abs(deltaY) > danger;  // It is "out of danger"
        }

        public override string ToString()
        {
            return "ChaseEnemyTransition";
        }

    }
}
