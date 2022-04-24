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
            int id;
            Vector3Int obj; creature.Enemy(out id, out obj);
            int deltaX = obj.x - creature.x,       // Direction of objective
                deltaY = obj.y - creature.y;

            return Math.Abs(deltaX) <= UniverseParametersManager.parameters.adjacentLength &&   // This implies inside attacking range
                Math.Abs(deltaY) <= UniverseParametersManager.parameters.adjacentLength &&
                creature.CanReach((Entities.Creature.HeightLayer)obj.z);    // This assumes an airborne creature can hit every layer below it, and so on
        }

        public override string ToString()
        {
            return "AttackTransition";
        }
    }
}
