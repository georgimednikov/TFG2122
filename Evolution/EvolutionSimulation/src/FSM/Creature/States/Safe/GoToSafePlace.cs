using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// The creature moves to a safe place (to sleep in a safe place)
    /// </summary>
    class GoToSafePlace : CreatureState
    {
        public GoToSafePlace(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return creature.GetNextCostOnPath();
        }

        public override void Action()
        {
            creature.SetPath(creature.GetClosestSafePlace().Item1, creature.GetClosestSafePlace().Item2);
            Vector3 nextPos = creature.GetNextPosOnPath();
            creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
            Console.WriteLine("GoToSafePlace action");
        }

        public override string ToString()
        {
            return "GoToSafePlaceState";
        }
    }
}
