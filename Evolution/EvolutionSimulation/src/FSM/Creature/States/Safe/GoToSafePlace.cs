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

        public override void OnEntry()
        {
            creature.SetPath(creature.GetClosestSafePlace().Item1, creature.GetClosestSafePlace().Item2);
        }

        public override void Action()
        {
            Vector3 nextPos = creature.GetNextPosOnPath();
            if(nextPos.X != -1 || nextPos.Y != -1 || nextPos.Z != -1)
                creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
            Console.WriteLine("GoToSafePlace action");
            creature.SetPath(creature.GetClosestSafePlace().Item1, creature.GetClosestSafePlace().Item2);
        }

        public override string ToString()
        {
            return "GoToSafePlaceState";
        }
    }
}
