using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// Hide from an enemy
    /// </summary>
    class Hide : CreatureState
    {
        public Hide(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return (int)(1000 * ((200f - creature.stats.GroundSpeed) / 100f));
        }

        public override void Action()    // TODO: Que es esconderse?
        {
            Console.WriteLine("Hide action");
        }

        public override string ToString()
        {
            return "HideState";
        }
    }
}
