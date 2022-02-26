using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// Runs away from an enemy
    /// </summary>
    class Fleeing : CreatureState
    {
        // How costly it is to move compared to regular safe movement
        private float modifier = 1.25f;

        public Fleeing(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return (int)(1000 * ((200f - creature.stats.GroundSpeed) / 100f) * modifier);
        }

        public override void Action()
        {
            // If the creature is in a different tile, simpli get away from it
            int oX = creature.nearestEnemy.x,   // Objective's position
                oY = creature.nearestEnemy.y;
            int deltaX = oX - creature.x,       // Direction of opposite movement
                deltaY = oY - creature.y;
            int normX = deltaX / Math.Abs(deltaX),  // Normalized direction of movement 
                normY = deltaY / Math.Abs(deltaY);  // as you can only move once per actions (nut can have multiple actions per tick)

            if (creature.x == creature.nearestEnemy.x && creature.y == creature.nearestEnemy.y) // If it is in the same tile, go in a random direction
                do {
                    normX = RandomGenerator.Next(-1, 2);
                    normY = RandomGenerator.Next(-1, 2);
                } while (normX == 0 && normY == 0);
            
            if (creature.world.canMove(creature.x - normX, creature.x - normY))
            {
                creature.Place(creature.x + normX, creature.x + normY);
            }
        }

        public override string ToString()
        {
            return "FleeingState";
        }
    }
}
