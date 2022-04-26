using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// A male creature moves to a possible mate with the goal of reproduction
    /// </summary>
    class GoToMate : CreatureState
    {
        Vector3Int matePos;
        int mateid;
        bool hasMateAndNotInSamePos;

        public GoToMate(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return creature.GetNextCostOnPath();
        }

        public override void OnEntry()
        {
            // If the mate is has not spontaneously died or the creature forgor, and it isn not already there; it can set the path to the mate
            hasMateAndNotInSamePos = creature.Mate(out mateid, out matePos) && (matePos.x != creature.x || matePos.y != creature.y ||
                creature.creatureLayer != (Entities.Creature.HeightLayer)matePos.z);
            if (hasMateAndNotInSamePos)
                creature.SetPath(matePos.x, matePos.y, (Entities.Creature.HeightLayer)matePos.z);        
        
        }

        /// <summary>
        /// Go to the closest possible mate
        /// </summary>
        public override void Action()
        {
            if (hasMateAndNotInSamePos)
            {
                Vector3 nextPos = creature.GetNextPosOnPath();
                if (nextPos.X != -1 && nextPos.Y != -1) 
                    creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);

                Vector3Int tmpPos;
                bool hasMate = creature.Mate(out mateid, out tmpPos);
                if (nextPos.X == -1)
                {
                    creature.SetPath(tmpPos.x, tmpPos.y, (Entities.Creature.HeightLayer)tmpPos.z);
                }

                // If the mate position changed (i.e. other better mate is near or the mate has changed its position), the creature updates its destiny.
                if (hasMate && matePos != tmpPos)
                {
                    matePos.x = tmpPos.x;
                    matePos.y = tmpPos.y;
                    matePos.z = tmpPos.z;
                    // Check if the new mate is not already at the creature position
                    hasMateAndNotInSamePos = matePos.x != creature.x || matePos.y != creature.y || creature.creatureLayer != (Entities.Creature.HeightLayer)matePos.z;
                    if (hasMateAndNotInSamePos)
                        creature.SetPath(matePos.x, matePos.y, (Entities.Creature.HeightLayer)matePos.z);
                }
                else
                    hasMateAndNotInSamePos = hasMate && (matePos.x != creature.x || matePos.y != creature.y || creature.creatureLayer != (Entities.Creature.HeightLayer)matePos.z);
            }
        }

        public override string GetInfo()
        {
            return creature.speciesName + " with ID: " + creature.ID + " IN (" + creature.x + ", " + creature.y + ", " + creature.creatureLayer + ") GOES TO MATE AT (" + matePos.x + ", " + matePos.y + ", " + matePos.z +")";
        }
        public override string ToString()
        {
            return "GoToMateState";
        }
    }
}
