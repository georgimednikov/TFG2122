﻿namespace EvolutionSimulation.FSM.Creature.States
{
    class Sleeping : CreatureState
    {
        public Sleeping(Entities.Creature c) : base(c) { creature = c; }

        // This move is energy netural, costing the same nergy that is obtained in a tick
        public override bool canPerformAction(int actionPoints)
        {
            return actionPoints >= 10 * creature.stats.Metabolism;
        }

        // Increases current rest
        public override int Action()
        {
            creature.stats.CurrRest += creature.stats.RestRecovery;
            if (creature.stats.CurrRest > creature.stats.MaxRest) creature.stats.CurrRest = creature.stats.MaxRest;
            return 10 * creature.stats.Metabolism; // Cost of the action performed
        }

        public override string ToString()
        {
            return "SleepingState";
        }
    }
}
