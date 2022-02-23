using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// Hide from an enemy
    /// </summary>
    class Hide : CreatureState
    {
        public Hide(Entities.Creature c) : base(c) { creature = c; }

        public override bool canPerformAction(int actionPoints)
        {
            return actionPoints >= 1000 * ((200f - creature.stats.GroundSpeed) / 100f);
        }

        public override int Action()
        {
            Console.WriteLine("Hide action");
            return 0;
        }

        public override string ToString()
        {
            return "HideState";
        }
    }
}
