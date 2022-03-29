using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// The creature moves to a safe place (to sleep in a safe place)
    /// </summary>
    class GoToSafePlace : CreatureState
    {
        Vector2Int safePos;
        bool hasSafePosAndNotInSamePos;
        public GoToSafePlace(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return creature.GetNextCostOnPath();
        }

        public override void OnEntry()
        {
            safePos = creature.SafePosition();
            hasSafePosAndNotInSamePos = safePos != null && (safePos.x != creature.x || safePos.y != creature.y);
            if (hasSafePosAndNotInSamePos)
                creature.SetPath(safePos.x, safePos.y);
        }

        public override void Action()
        {
            // If no safe position is found or the creature is already at the safe position, other transition must trigger the next tick
            if (hasSafePosAndNotInSamePos)  
            {
                Vector3 nextPos = creature.GetNextPosOnPath();
                if (nextPos.X != -1 && nextPos.Y != -1) // TODO: no haria falta
                    creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
                
                Vector2Int tmpPos = creature.SafePosition();
                bool hasSafePos = tmpPos != null;
                bool notAtDestiny = tmpPos.x != creature.x || tmpPos.y != creature.y;
                // If the safe position changed (i.e. other better safe pos is near), the creature updates its destiny.
                if (hasSafePos && tmpPos != safePos)
                {
                    safePos.x = tmpPos.x;
                    safePos.y = tmpPos.y;
                    // Check if the new safe pos is not already at the creature position
                    if (notAtDestiny)
                        creature.SetPath(safePos.x, safePos.y);
                }
                hasSafePosAndNotInSamePos = hasSafePos && notAtDestiny;
            }
        }

        public override string GetInfo()
        {
            return creature.speciesName + " with ID: " + creature.ID + " IN (" + creature.x + ", " + creature.y + ")GOES TO SAFE PLACE AT (" + safePos.x + ", " + safePos.y + ")";
        }
        public override string ToString()
        {
            return "GoToSafePlaceState";
        }
    }
}
