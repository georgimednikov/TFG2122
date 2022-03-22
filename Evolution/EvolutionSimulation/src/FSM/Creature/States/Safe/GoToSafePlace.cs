﻿using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// The creature moves to a safe place (to sleep in a safe place)
    /// </summary>
    class GoToSafePlace : CreatureState
    {
        Vector2Int safePos;

        public GoToSafePlace(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return creature.GetNextCostOnPath();
        }

        public override void OnEntry()
        {
            safePos = creature.SafePosition();
            creature.SetPath(safePos.x, safePos.y);
        }

        public override void Action()
        {
            Vector3 nextPos = creature.GetNextPosOnPath();
            if(nextPos.X != -1 || nextPos.Y != -1 || nextPos.Z != -1)
                creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
            if (safePos != creature.SafePosition())
            {
                safePos = creature.SafePosition();
                creature.SetPath(safePos.x, safePos.y);
            }
            Console.WriteLine(creature.speciesName + " GOES TO SAFE PLACE (" + creature.x + ", " + creature.y + ")");
        }


        public override string GetInfo()
        {
            return safePos.ToString();
        }
        public override string ToString()
        {
            return "GoToSafePlaceState";
        }
    }
}
