﻿using System;
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
            modifier = 1.1f - (creature.stats.GroundSpeed / c.chromosome.GetFeatureMax(Genetics.CreatureFeature.Mobility) * UniverseParametersManager.parameters.fleeingCostMultiplier);
        }

        public override void OnEntry()
        {
            Vector2Int objective; creature.Menace(out _, out objective);
            dngX = objective.x;
            dngY = objective.y;
            pathX = 0;
            pathY = 0;
            // It either seeks its allies or runs from its enemy
            Vector2Int fwiend = creature.GetClosestAllyPosition();
            if(fwiend != null) {
                pathX = fwiend.x;
                pathY = fwiend.y;
            } else positionAwayFromMe(ref pathX, ref pathY);

            if (pathX == creature.x && pathY == creature.y)
                creature.cornered = true;
            else
                creature.SetPath(pathX, pathY);

            creature.CreateDanger();
        }

        public override int GetCost()   // TODO: con 0 de coste podria funcionar por el cornered
        {
            int cost = creature.GetNextCostOnPath();
            if(cost < 0) {
                creature.cornered = true;
                return UniverseParametersManager.parameters.baseActionCost;
            } else return creature.cornered? UniverseParametersManager.parameters.baseActionCost : (int)(cost * modifier);
        }

        // TODO: Esto no deberia estar aqui!!!
        public void positionAwayFromMe(ref int fX, ref int fY)
        {
            // If the creature is in a different tile, simpli get away from it
            int deltaX = dngX - creature.x,       // Direction of opposite movement
                deltaY = dngY - creature.y;
            int normX = deltaX == 0 ? 0 : deltaX / Math.Abs(deltaX),  // Normalized direction of movement 
                normY = deltaY == 0 ? 0 : deltaY / Math.Abs(deltaY);  // as you can only move once per action (but can have multiple actions per tick)

            Vector2Int position; creature.Menace(out _, out position);
            if (creature.x == position.x && creature.y == position.y) // If it is in the same tile, go in a random direction
                do
                {
                    normX = RandomGenerator.Next(-1, 2);
                    normY = RandomGenerator.Next(-1, 2);
                } while (normX == 0 && normY == 0);

            int xSum = 0, ySum = 0;
            // Attempts to find the point furthest away from attacker
            while (Math.Abs(xSum) <= creature.stats.Perception && Math.Abs(ySum) <= creature.stats.Perception)
            {
                if (creature.world.canMove(creature.x + xSum, creature.y + ySum))
                {
                    fX = creature.x + xSum;
                    fY = creature.y + ySum;
                }
                xSum -= normX;
                ySum -= normY;
            }
        }

        public override void Action()
        {
            if (!creature.cornered)
            {
                Vector3 nextPos = creature.GetNextPosOnPath();
                if (nextPos.X != -1 || nextPos.Y != -1 || nextPos.Z != -1)
                    creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
            }

            // Attempts to see if the escape route has changed
            Vector2Int objective; creature.Menace(out _, out objective);
            if (dngX != objective.x || dngY != objective.y)  // If it has changed, reassign the path
            {
                dngX = objective.x;
                dngY = objective.y;
                // It either seeks its allies or runs from its enemy
                Vector2Int fwiend = creature.GetClosestAllyPosition();
                if (fwiend != null) {
                    pathX = fwiend.x;
                    pathY = fwiend.y;
                }
                else positionAwayFromMe(ref pathX, ref pathY);
                if (pathX == creature.x && pathY == creature.y)
                    creature.cornered = true;
                else
                    creature.SetPath(pathX, pathY);
            }
            Console.WriteLine(creature.speciesName + " FLEES (" + creature.x + ", " + creature.y + ")");
        }

        public override string GetInfo()
        {
            return "Objective: " + pathX + ", " + pathY;
        }

        public override string ToString()
        {
            return "FleeingState";
        }
    }
}
