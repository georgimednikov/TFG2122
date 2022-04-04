using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// A male creature moves to a possible mate with the goal of reproduction
    /// </summary>
    class GoToMate : CreatureState
    {
        Vector2Int matePos;
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
                creature.creatureLayer != creature.world.GetCreature(mateid).creatureLayer);
            if (hasMateAndNotInSamePos)
                creature.SetPath(matePos.x, matePos.y, creature.world.GetCreature(mateid).creatureLayer);        
        
        }

        /// <summary>
        /// Go to the closest possible mate
        /// </summary>
        public override void Action()
        {
            if (hasMateAndNotInSamePos)
            {
                Vector3 nextPos = creature.GetNextPosOnPath();
                if (nextPos.X != -1 && nextPos.Y != -1) // TODO: no haria falta creo
                    creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
                else if (nextPos.X == -2)
                {
                    creature.SetPath(matePos.x, matePos.y);

                }
                Vector2Int tmpPos;
                bool hasMate = creature.Mate(out mateid, out tmpPos);
                // If the mate position changed (i.e. other better mate is near or the mate has changed its position), the creature updates its destiny.
                if (hasMate && matePos != tmpPos)
                {
                    matePos.x = tmpPos.x;
                    matePos.y = tmpPos.y;
                    // Check if the new mate is not already at the creature position
                    hasMateAndNotInSamePos = matePos.x != creature.x || matePos.y != creature.y || creature.creatureLayer != creature.world.GetCreature(mateid).creatureLayer;
                    if (hasMateAndNotInSamePos)
                        creature.SetPath(matePos.x, matePos.y);
                }
                else
                    hasMateAndNotInSamePos = hasMate && (matePos.x != creature.x || matePos.y != creature.y || creature.creatureLayer != creature.world.GetCreature(mateid).creatureLayer);
            }
        }

        public override string GetInfo()
        {
            return creature.speciesName + " with ID: " + creature.ID + " IN (" + creature.x + ", " + creature.y + ") GOES TO MATE AT (" + matePos.x + ", " + matePos.y + ")";
        }
        public override string ToString()
        {
            return "GoToMateState";
        }
    }
}
