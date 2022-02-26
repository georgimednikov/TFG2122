using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    // TODO: Estado para testear, hacer el estado correctamente
    class ChaseEnemy : CreatureState
    {
        // How costly it is to move compared to regular safe movement
        private float modifier = 1.25f;

        public ChaseEnemy(Entities.Creature c) : base(c) { creature = c; }

        public override bool canPerformAction(int actionPoints)
        {
            return actionPoints >= (1000 * ((200f - creature.stats.GroundSpeed) / 100f) * modifier);
        }

        public override int Action()
        {
            int oX = creature.nearestEnemy.x,   // Objective's position
                oY = creature.nearestEnemy.y;
            int deltaX = oX - creature.x,       // Direction of movement
                deltaY = oY - creature.y;
            int normX = deltaX / Math.Abs(deltaX),  // Normalized direction of movement 
                normY = deltaY / Math.Abs(deltaY);  // as you can only move once per actions (nut can have multiple actions per tick)
            
            if (creature.world.canMove(creature.x + normX, creature.x + normY))
            {
                creature.Place(creature.x + normX, creature.x + normY);
                return (int)(1000 * ((200f - creature.stats.GroundSpeed) / 100f) * modifier); // Cost of the action performed
            }
            return 0;
        }

        public override string ToString()
        {
            return "ChaseEnemyState";
        }
    }
}
