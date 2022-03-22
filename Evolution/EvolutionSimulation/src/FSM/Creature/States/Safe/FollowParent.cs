using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// A child creature follow it mother or father
    /// </summary>
    class FollowParent : CreatureState
    {
        Vector2Int parentPos;

        public FollowParent(Entities.Creature c) : base(c) { creature = c; }
        
        public override int GetCost()
        {
            return creature.GetNextCostOnPath();
        }

        public override void OnEntry()
        {
            SetPos();
        }

        /// <summary>
        /// Go to the parent position
        /// </summary>
        public override void Action()
        {
            Vector3 nextPos = creature.GetNextPosOnPath();
            if (nextPos.X != -1 || nextPos.Y != -1 || nextPos.Z != -1)
                creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
            SetPos();
            Console.WriteLine(creature.speciesName + " FOLLOWS PARENT (" + creature.x + ", " + creature.y + ")");
        }
        public override string GetInfo()
        {
            return parentPos.ToString();
        }

        public override string ToString()
        {
            return "FollowParentState";
        }

        private void SetPos()
        {
            if (creature.Parent(out _, out parentPos))
                creature.SetPath(parentPos.x, parentPos.y);
            else//just in case the creature does not have a parent
                creature.SetPath(creature.x, creature.y);
        }
    }
}
