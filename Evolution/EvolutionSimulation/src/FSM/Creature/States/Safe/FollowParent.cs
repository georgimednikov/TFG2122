﻿using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// A child creature follow it mother or father
    /// </summary>
    class FollowParent : CreatureState
    {

        public FollowParent(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return creature.GetNextCostOnPath();
        }

        public override void OnEntry()
        {
            creature.SetPath(creature.parentToFollow.x, creature.parentToFollow.y);
        }

        /// <summary>
        /// Go to the parent position
        /// </summary>
        public override void Action()
        {
            Console.WriteLine("FollowParent action");
            Vector3 nextPos = creature.GetNextPosOnPath();
            if (nextPos.X != -1 || nextPos.Y != -1 || nextPos.Z != -1)
                creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
            creature.SetPath(creature.parentToFollow.x, creature.parentToFollow.y);
        }

        public override string ToString()
        {
            return "FollowParentState";
        }
    }
}