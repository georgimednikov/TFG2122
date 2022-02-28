using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// Runs away from an enemy
    /// </summary>
    class Fleeing : CreatureState
    {
        // How costly it is to move compared to regular safe movement
        private float modifier;

        public Fleeing(Entities.Creature c) : base(c) 
        { 
            creature = c;
            modifier = 1.1f - (creature.stats.GroundSpeed / 150f);  // TODO: Que dependa bien de stats
        }

        public override int GetCost()
        {
            return (int)(1000 * ((200f - creature.stats.GroundSpeed) / 100f) * modifier);
        }

        public override void Action()
        {
            // If the creature is in a different tile, simpli get away from it
            int oX = creature.GetClosestCreature().x,   // Objective's position
                oY = creature.GetClosestCreature().y;
            int deltaX = oX - creature.x,       // Direction of opposite movement
                deltaY = oY - creature.y;
            int normX = deltaX == 0 ? 0 : deltaX / Math.Abs(deltaX),  // Normalized direction of movement 
                normY = deltaY == 0 ? 0 : deltaY / Math.Abs(deltaY);  // as you can only move once per actions (nut can have multiple actions per tick)

            if (creature.x == creature.GetClosestCreature().x && creature.y == creature.GetClosestCreature().y) // If it is in the same tile, go in a random direction
                do {
                    normX = RandomGenerator.Next(-1, 2);
                    normY = RandomGenerator.Next(-1, 2);
                } while (normX == 0 && normY == 0);
            
            if (creature.world.canMove(creature.x - normX, creature.y - normY))
            {
                creature.Place(creature.x - normX, creature.y - normY);
            }
        }

        public override string ToString()
        {
            return "FleeingState";
        }
    }
}
