using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// When the creature has to do something but doesn't know 
    /// where it can do it, i.e. the creature wants to drink but it 
    /// doesn't know where there is a mass of water. 
    /// 
    /// It moves around the world improving its knowledge about it,
    /// updating its mental map of the world
    /// </summary>
    class Explore : CreatureState
    {
        Vector3 posToDiscover;
        int regionID;
        public Explore(Entities.Creature c) : base(c)
        {
            creature = c;
        }

        public override int GetCost()
        {
            return creature.GetNextCostOnPath();
        }

        public override void OnEntry()
        {
            regionID = creature.NewExploreRegion();
            posToDiscover = new Vector3(creature.world.regionMap[regionID].spawnPoint, 0); // Region spawn point is always at ground height
            creature.SetPath((int)posToDiscover.X, (int)posToDiscover.Y, Entities.Creature.HeightLayer.Ground);
        }

        public override void Action()
        {
            Vector3 nextPos = creature.GetNextPosOnPath();
            creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
            // If the creature already arrived at the path, the next cost is -1 or 0, so the creature searches for other path
            if (creature.GetNextCostOnPath() <= 0)
            {
                // Region changed but destiny not reached still
                if (nextPos != posToDiscover)
                    creature.SetPath((int)posToDiscover.X, (int)posToDiscover.Y, Entities.Creature.HeightLayer.Ground);

                // Destiny reached, a new destiny in an unexplored region is setted
                else
                {
                    regionID = creature.NewExploreRegion();
                    posToDiscover = new Vector3(creature.world.regionMap[regionID].spawnPoint, 0); // Region spawn point is always at ground height
                    creature.SetPath((int)posToDiscover.X, (int)posToDiscover.Y, Entities.Creature.HeightLayer.Ground);
                }
            }
        }

        public override string GetInfo()
        {
            return creature.speciesName + " with ID: " + creature.ID + " IN REGION: " 
                + creature.world.map[creature.x, creature.y].regionId + " IN (" + creature.x + ", " + creature.y + ", " + creature.creatureLayer + ")" 
                + " EXPLORES AT (" + posToDiscover.X + ", " + posToDiscover.Y + ") REGION: " + regionID;
        }

        public override string ToString()
        {
            return "ExploreState";
        }
    }
}
