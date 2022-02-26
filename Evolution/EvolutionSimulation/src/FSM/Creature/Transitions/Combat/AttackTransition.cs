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
            int oX = creature.nearestEnemy.x,   // Objective's position
                oY = creature.nearestEnemy.y;
            int deltaX = oX - creature.x,       // Direction of objective
                deltaY = oY - creature.y;

            return deltaX <= 1 || deltaY <= 1;  // This implies inisde melee range
        }

        public override string ToString()
        {
            return "AttackTransition";
        }

    }
}
