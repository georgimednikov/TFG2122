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
            int id;
            Vector3Int obj; creature.Enemy(out id, out obj);
            int deltaX = obj.x - creature.x,       // Direction of objective
                deltaY = obj.y - creature.y;

            return (Math.Abs(deltaX) > UniverseParametersManager.parameters.adjacentLength ||   // This implies outside of melee range
                Math.Abs(deltaY) > UniverseParametersManager.parameters.adjacentLength) ||
                !creature.CanReach((Entities.Creature.HeightLayer)obj.z);   // If enemy cannot be reached, it will stalk and wait for it to get lower
        }

        public override string ToString()
        {
            return "ChaseEnemyTransition";
        }

    }
}
