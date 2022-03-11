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
            Vector2Int posToDiscover = creature.GetUndiscoveredPlacePosition();
            creature.SetPath(posToDiscover.x, posToDiscover.y);
        }

        public override void Action()
        {
            Vector3 nextPos = creature.GetNextPosOnPath();
            if (nextPos.X < 0 || nextPos.Y < 0)
            {
                Vector2Int posToDiscover = creature.GetUndiscoveredPlacePosition();
                creature.SetPath(posToDiscover.x, posToDiscover.y);
                nextPos = creature.GetNextPosOnPath();
            }
            creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
            Console.WriteLine(creature.speciesName + " EXPLORES (" + creature.x + ", " + creature.y + ")");
        }

        public override string ToString()
        {
            return "ExploreState";
        }
    }
}
