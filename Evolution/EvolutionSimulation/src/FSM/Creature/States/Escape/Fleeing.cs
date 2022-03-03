using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// Runs away from an enemy
    /// </summary>
    class Fleeing : CreatureState
    {
        // How costly it is to move compared to regular safe movement
        private float modifier;
        // Position of the danger
        int dngX, dngY;
        // Objective of the escape path
        int pathX, pathY;

        public Fleeing(Entities.Creature c) : base(c) 
        { 
            creature = c;
            modifier = 1.1f - (creature.stats.GroundSpeed / 150f);  // TODO: Que dependa bien de stats
        }

        public override void OnEntry()
        {
            Entities.Creature objective = creature.GetClosestCreature();
            dngX = objective.x;
            dngY = objective.y;
            pathX = 0;
            pathY = 0;
            positionAwayFromMe(ref pathX, ref pathY);
            creature.SetPath(pathX, pathY);   // This MUST be set up for the cost of the action to work
        }

        public override int GetCost()
        {
            return (int)(creature.GetNextCostOnPath() * modifier);
        }

        // TODO: Esto no deberia estar aqui!!!
        public void positionAwayFromMe(ref int fX, ref int fY)
        {
            // If the creature is in a different tile, simpli get away from it
            int deltaX = dngX - creature.x,       // Direction of opposite movement
                deltaY = dngY - creature.y;
            int normX = deltaX == 0 ? 0 : deltaX / Math.Abs(deltaX),  // Normalized direction of movement 
                normY = deltaY == 0 ? 0 : deltaY / Math.Abs(deltaY);  // as you can only move once per action (but can have multiple actions per tick)

            if (creature.x == creature.GetClosestCreature().x && creature.y == creature.GetClosestCreature().y) // If it is in the same tile, go in a random direction
                do
                {
                    normX = RandomGenerator.Next(-1, 2);
                    normY = RandomGenerator.Next(-1, 2);
                } while (normX == 0 && normY == 0);

            int xSum = 0, ySum = 0;
            while (creature.world.canMove(creature.x + xSum - normX, creature.y + ySum - normY))  // Attempts to find the point furthest aay from attacker
            {
                xSum -= normX;
                ySum -= normY;
            }

            fX = creature.x + xSum;
            fY = creature.y + ySum;
        }

        public override void Action()
        {
            Vector3 nextPos = creature.GetNextPosOnPath();
            creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);

            // Attempts to see if the escape route has changed
            Entities.Creature objective = creature.GetClosestCreature();
            if(dngX != objective.x || dngY != objective.y)  // If it has changed, reassign the path
            {
                dngX = objective.x;
                dngY = objective.y;
                positionAwayFromMe(ref pathX, ref pathY);
                creature.SetPath(pathX, pathY);
            }
        }

        public override string ToString()
        {
            return "FleeingState";
        }
    }
}
