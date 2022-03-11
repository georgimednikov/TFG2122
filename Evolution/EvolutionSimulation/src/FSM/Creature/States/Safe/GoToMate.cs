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

        public GoToMate(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return creature.GetNextCostOnPath();
        }

        public override void OnEntry()
        {
            matePos = creature.GetClosestPossibleMatePosition();
            creature.SetPath(matePos.x, matePos.y);
        }

        /// <summary>
        /// Go to the closest possible mate
        /// </summary>
        public override void Action()
        {
            Console.WriteLine("GoToMate action");
            Vector3 nextPos = creature.GetNextPosOnPath();
            if (nextPos.X != -1 || nextPos.Y != -1 || nextPos.Z != -1)
                creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);

            if (matePos != creature.GetClosestPossibleMatePosition())
            {
                matePos = creature.GetClosestPossibleMatePosition();
                creature.SetPath(matePos.x, matePos.y);
            }
        }

        public override string ToString()
        {
            return "GoToMateState";
        }
    }
}
