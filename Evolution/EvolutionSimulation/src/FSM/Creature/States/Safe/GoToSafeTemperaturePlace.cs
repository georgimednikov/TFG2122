using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// The creature moves to a safe temperature place 
    /// </summary>
    class GoToSafeTemperaturePlace : CreatureState
    {
        Vector2Int safeTempPos;
        bool hasSafeTempPosAndNotInSamePos;
        public GoToSafeTemperaturePlace(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return creature.GetNextCostOnPath();
        }

        public override void OnEntry()
        {
            safeTempPos = creature.SafeTemperaturePosition();
           
            hasSafeTempPosAndNotInSamePos = safeTempPos != null && (safeTempPos.x != creature.x || safeTempPos.y != creature.y || creature.creatureLayer != 0);
            if(hasSafeTempPosAndNotInSamePos)
                creature.SetPath(safeTempPos.x, safeTempPos.y);
        }

        public override void Action()
        {
            // If no safe temperature position is found or the creature is already at the safe temperature position, other transition must trigger the next tick
            if (hasSafeTempPosAndNotInSamePos)  
            {
                Vector3 nextPos = creature.GetNextPosOnPath();
                if (nextPos.X != -1 && nextPos.Y != -1)
                    creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);

                if(nextPos.X == -1)
                {
                    creature.SetPath(safeTempPos.x, safeTempPos.y);
                }
                Vector2Int tmpPos = creature.SafeTemperaturePosition();
                bool hasSafePos = tmpPos != null;
                // If the safe temperature position changed (i.e. other better safe pos is near), the creature updates its destiny.
                if (hasSafePos && tmpPos != safeTempPos)
                {
                    safeTempPos.x = tmpPos.x;
                    safeTempPos.y = tmpPos.y;
                    hasSafeTempPosAndNotInSamePos = safeTempPos.x != creature.x || 
                        safeTempPos.y != creature.y || creature.creatureLayer != 0;
                    // Check if the new safe pos is not already at the creature position
                    if (hasSafeTempPosAndNotInSamePos)
                        creature.SetPath(safeTempPos.x, safeTempPos.y);
                }
                else
                    hasSafeTempPosAndNotInSamePos = hasSafePos && (safeTempPos.x != creature.x || safeTempPos.y != creature.y || creature.creatureLayer != 0);
            }
        }

        public override string GetInfo()
        {
            return creature.speciesName + " with ID: " + creature.ID + " IN (" + creature.x + ", " + creature.y + ", " + creature.creatureLayer + ")GOES TO SAFE TEMPERATURE PLACE AT (" + safeTempPos.x + ", " + safeTempPos.y + ")";
        }
        public override string ToString()
        {
            return "GoToSafeTemperaturePlaceState";
        }
    }
}
