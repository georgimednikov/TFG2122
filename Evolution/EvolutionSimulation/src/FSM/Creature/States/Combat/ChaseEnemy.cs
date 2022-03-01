using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    class ChaseEnemy : CreatureState
    {
        // How costly it is to move compared to regular safe movement
        private float modifier; 

        public ChaseEnemy(Entities.Creature c) : base(c) 
        { 
            creature = c;
            modifier = 1.1f - (c.stats.Aggressiveness / 20f) * 0.4f; // TODO: Modificador que dependa bien, ahora mismo a mas agresividad mejor persigue
        }

        public override int GetCost()
        {
            return (int)(1000 * ((200f - creature.stats.GroundSpeed) / 100f) * modifier);
        }

        public override void Action()
        {
            int oX = creature.GetClosestCreatureReachable().x,   // Objective's position
                oY = creature.GetClosestCreatureReachable().y;
            int deltaX = oX - creature.x,       // Direction of movement
                deltaY = oY - creature.y;
            int normX = deltaX == 0 ? 0 : deltaX / Math.Abs(deltaX),  // Normalized direction of movement 
                normY = deltaY == 0 ? 0 : deltaY / Math.Abs(deltaY);  // as you can only move once per actions (nut can have multiple actions per tick)
            
            if (creature.world.canMove(creature.x + normX, creature.y + normY))
            {
                creature.Place(creature.x + normX, creature.y + normY);
            }
        }

        public override string ToString()
        {
            return "ChaseEnemyState";
        }
    }
}
