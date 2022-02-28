using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class ChaseEnemyTransition : CreatureTransition
    {
        public ChaseEnemyTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            int oX = creature.GetClosestCreatureReachable().x,   // Objective's position
                oY = creature.GetClosestCreatureReachable().y;
            int deltaX = oX - creature.x,       // Direction of objective
                deltaY = oY - creature.y;

            return Math.Abs(deltaX) > 1 || Math.Abs(deltaY) > 1;    // This implies outside of melee range
        }

        public override string ToString()
        {
            return "ChaseEnemyTransition";
        }

    }
}
