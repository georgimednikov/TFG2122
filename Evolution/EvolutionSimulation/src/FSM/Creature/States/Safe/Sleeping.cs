using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    class Sleeping : CreatureState
    {
        public Sleeping(Entities.Creature c) : base(c) { creature = c; }

        // This move is energy netural, costing the same nergy that is obtained in a tick
        public override int GetCost()
        {
            return 10 * creature.stats.Metabolism;
        }

        // Increases current rest
        public override void Action()
        {
            Console.WriteLine("Sleeping");

            creature.stats.CurrRest += creature.stats.RestRecovery;
            if (creature.stats.CurrRest > creature.stats.MaxRest)
                creature.stats.CurrRest = creature.stats.MaxRest;
        }

        public override string ToString()
        {
            return "SleepingState";
        }
    }
}
