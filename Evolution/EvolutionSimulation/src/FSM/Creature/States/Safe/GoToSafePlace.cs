using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// The creature moves to a safe place (to sleep in a safe place)
    /// </summary>
    class GoToSafePlace : CreatureState
    {
        Tuple<int, int> safePos;

        public GoToSafePlace(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return creature.GetNextCostOnPath();
        }

        public override void OnEntry()
        {
            safePos = creature.GetClosestSafePlacePosition();
            creature.SetPath(safePos.Item1, safePos.Item2);
        }

        public override void Action()
        {
            Console.WriteLine("GoToSafePlace action");
            Vector3 nextPos = creature.GetNextPosOnPath();
            if(nextPos.X != -1 || nextPos.Y != -1 || nextPos.Z != -1)
                creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
            if (safePos != creature.GetClosestSafePlacePosition())
            {
                safePos = creature.GetClosestSafePlacePosition();
                creature.SetPath(safePos.Item1, safePos.Item2);
            }
        }

        public override string ToString()
        {
            return "GoToSafePlaceState";
        }
    }
}
