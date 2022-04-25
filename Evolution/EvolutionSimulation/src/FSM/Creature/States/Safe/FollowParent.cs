using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// A child creature follow it mother or father
    /// </summary>
    class FollowParent : CreatureState
    {
        Vector3Int parentPos;

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
            if (nextPos.X != -1 && nextPos.Y != -1 && creature.CanReach((Entities.Creature.HeightLayer)nextPos.Z))
                creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
            SetPos();
        }
        public override string GetInfo()
        {
            return creature.speciesName + " with ID: " + creature.ID + " IN (" + creature.x + ", " + creature.y + ") FOLLOWS PARENT AT (" + parentPos.x + ", " + parentPos.y + ")";
        }

        public override string ToString()
        {
            return "FollowParentState";
        }

        private void SetPos()
        {
            if (creature.Parent(out _, out parentPos))
                creature.SetPath(parentPos.x, parentPos.y, (Entities.Creature.HeightLayer)parentPos.z);
            else//just in case the creature does not have a parent
                creature.SetPath(creature.x, creature.y, (Entities.Creature.HeightLayer)parentPos.z);
        }
    }
}
