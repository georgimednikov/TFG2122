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
        Vector2Int posToDiscover;
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
            //do
            //{
            //} while (posToDiscover.x == creature.x && creature.y == posToDiscover.y);
            posToDiscover = creature.NewPosition();
            creature.SetPath(posToDiscover.x, posToDiscover.y);
        }

        public override void Action()
        {
            Vector3 nextPos = creature.GetNextPosOnPath();
            creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
            // If the creature already arrived at the path, the next cost is -1 or 0, so the creature searches for other path
            if (creature.GetNextCostOnPath() <= 0)
            {
                //do
                //{
                //} while (posToDiscover.x == creature.x && creature.y == posToDiscover.y);
                // The new position cannot be a 
                posToDiscover = creature.NewPosition();
                creature.SetPath(posToDiscover.x, posToDiscover.y);
            }
        }

        public override string GetInfo()
        {
            return creature.speciesName + " with ID: " + creature.ID + " IN (" + creature.x + ", " + creature.y + ")" + " EXPLORES AT (" + posToDiscover.x + ", " + posToDiscover.y + ")";
        }

        public override string ToString()
        {
            return "ExploreState";
        }
    }
}
