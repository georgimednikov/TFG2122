using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// A male creature moves to a possible mate with the goal of reproduction
    /// </summary>
    class GoToMate : CreatureState
    {
        public GoToMate(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return creature.GetNextCostOnPath();
        }

        /// <summary>
        /// Go to the closest possible mate
        /// </summary>
        public override void Action()
        {
            creature.SetPath(creature.GetClosestPossibleMate().x, creature.GetClosestPossibleMate().y);
            Vector3 nextPos = creature.GetNextPosOnPath();
            creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
            Console.WriteLine("GoToMate action");
        }

        public override string ToString()
        {
            return "GoToMateState";
        }
    }
}
