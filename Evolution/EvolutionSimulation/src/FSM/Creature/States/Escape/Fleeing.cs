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
        int pathX, pathY, pathZ;

        public Fleeing(Entities.Creature c) : base(c)
        {
            creature = c;
            modifier = 1.1f - (creature.stats.GroundSpeed / c.chromosome.GetFeatureMax(Genetics.CreatureFeature.Mobility) * 
                UniverseParametersManager.parameters.fleeingCostMultiplier);
        }

        public override int GetCost()
        {
            int cost = creature.GetNextCostOnPath();
            if(cost <= 0) {
                creature.cornered = true;
                return UniverseParametersManager.parameters.baseActionCost;
            } else return creature.cornered? UniverseParametersManager.parameters.baseActionCost : (int)(cost * modifier);
        }

        public override void OnEntry()
        {
            Vector3Int objective;
            int menaceId;
            creature.Menace(out menaceId, out objective);
            Entities.Creature objCreature = creature.world.GetCreature(menaceId);

#if TRACKER_ENABLED
            Telemetry.Tracker.Instance.Track(new Telemetry.Events.CreatureStateEntryNotSafe(creature.world.CurrentTick, creature.ID, creature.speciesName, ToString(), menaceId, creature.x, creature.y, objCreature == null ? " " : objCreature.speciesName));
#endif
            dngX = objective.x;
            dngY = objective.y;
            pathX = 0;
            pathY = 0;
            pathZ = 0;
            // It either seeks its allies or runs from its enemy
            Vector3Int fwiend;
            if(creature.Ally(out _, out fwiend) && CheckIfSafe(fwiend) &&
                (creature.CanReach((Entities.Creature.HeightLayer)fwiend.z) || creature.world.CanMove(fwiend.x, fwiend.y))) {
                pathX = fwiend.x;
                pathY = fwiend.y;
                pathZ = creature.CanReach((Entities.Creature.HeightLayer)fwiend.z) ? fwiend.z : 0;
                
            } else PositionAwayFromMe(ref pathX, ref pathY);

            if (pathX == creature.x && pathY == creature.y)
                creature.cornered = true;
            else
            {
                creature.SetPath(pathX, pathY, (Entities.Creature.HeightLayer)pathZ);
            }

            creature.CreateDanger();
        }

        public override void Action()
        {
            if (!creature.cornered && pathX != creature.x && pathY != creature.y)
            {
                Vector3 nextPos = creature.GetNextPosOnPath();
                if (nextPos.X != -1 && nextPos.Y != -1 && creature.CanReach((Entities.Creature.HeightLayer)nextPos.Z))
                    creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
            }

            // Attempts to see if the escape route has changed
            Vector3Int objective; creature.Menace(out _, out objective);
            if (dngX != objective.x || dngY != objective.y)  // If it has changed, reassign the path
            {
                dngX = objective.x;
                dngY = objective.y;
                pathZ = 0;
                // It either seeks its allies or runs from its enemy
                Vector3Int fwiend;
                if (creature.Ally(out _, out fwiend) && CheckIfSafe(fwiend) &&
                (creature.CanReach((Entities.Creature.HeightLayer)fwiend.z) || creature.world.CanMove(fwiend.x, fwiend.y)))
                {
                    pathX = fwiend.x;
                    pathY = fwiend.y;
                    pathZ = creature.CanReach((Entities.Creature.HeightLayer)fwiend.z) ? fwiend.z : 0;
                }
                else PositionAwayFromMe(ref pathX, ref pathY);


                if (pathX == creature.x && pathY == creature.y)
                    creature.cornered = true;
                else
                    creature.SetPath(pathX, pathY, (Entities.Creature.HeightLayer)pathZ);
            }
        }

        private void PositionAwayFromMe(ref int fX, ref int fY)
        {
            // If the creature is in a different tile, simpli get away from it
            int deltaX = dngX - creature.x,       // Direction of opposite movement
                deltaY = dngY - creature.y;
            int normX = deltaX == 0 ? 0 : deltaX / Math.Abs(deltaX),  // Normalized direction of movement 
                normY = deltaY == 0 ? 0 : deltaY / Math.Abs(deltaY);  // as you can only move once per action (but can have multiple actions per tick)

            Vector3Int position; creature.Menace(out _, out position);
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
                if (creature.world.CanMove(creature.x + xSum, creature.y + ySum))
                {
                    fX = creature.x + xSum;
                    fY = creature.y + ySum;
                }
                xSum -= normX;
                ySum -= normY;
            }
        }

        /// <summary>
        /// Returns if it is safe to go to a position
        /// In summary, if it would entail going through the current danger position.
        /// </summary>
        private bool CheckIfSafe(Vector3Int friendPos)
        {
            // Direction of possible ally
            int fwiendX = friendPos.x - creature.x,
                fwiendY = friendPos.y - creature.y;
            int fnormX = fwiendX == 0 ? 0 : fwiendX / Math.Abs(fwiendX),
                fnormY = fwiendY == 0 ? 0 : fwiendY / Math.Abs(fwiendY);
            // Direction of enemy
            int nmeX = dngX - creature.x,
                nmeY = dngY - creature.y;
            int nnormX = nmeX == 0 ? 0 : nmeX / Math.Abs(nmeX),  // Normalized direction of movement 
                nnormY = nmeY == 0 ? 0 : nmeY / Math.Abs(nmeY);  // as you can only move once per action (but can have multiple actions per tick)

            return (fnormX != nnormX || fnormY != nnormY);
        }

        public override string GetInfo()
        {
            return  creature.speciesName + " with ID: " + creature.ID + " AT (" + creature.x + ", " + creature.y + ", " + creature.creatureLayer + ") FLEES TO (" + pathX + ", " + pathY + ", " + pathZ +")";
        }

        public override string ToString()
        {
            return "FleeingState";
        }
    }
}
