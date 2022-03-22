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
            if(creature.Mate(out _, out matePos))
                creature.SetPath(matePos.x, matePos.y);
            else//just in case, that should not happend
                creature.SetPath(creature.x, creature.y);
        }

        /// <summary>
        /// Go to the closest possible mate
        /// </summary>
        public override void Action()
        {
            Vector3 nextPos = creature.GetNextPosOnPath();
            if (nextPos.X != -1 || nextPos.Y != -1 || nextPos.Z != -1)
                creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
            
            Vector2Int tmpPos;            
            if (creature.Mate(out _, out tmpPos) && matePos != tmpPos)
            {
                matePos.x = tmpPos.x;
                matePos.y = tmpPos.y;
                creature.SetPath(matePos.x, matePos.y);
            }
            Console.WriteLine(creature.speciesName + " GOES TO MATE (" + creature.x + ", " + creature.y + ")");
        }


        public override string GetInfo()
        {
            return matePos.ToString();
        }
        public override string ToString()
        {
            return "GoToMateState";
        }
    }
}
