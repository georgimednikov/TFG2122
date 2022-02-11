namespace EvolutionSimulation.FSM.Creature.States
{
    class Sleeping : CreatureState
    {
        public Sleeping(Entities.Creature c) : base(c) { creature = c; }

        // This move is energy netural, costing the same nergy that is obtained in a tick
        public override bool canPerformAction(int actionPoints)
        {
            return actionPoints >= 10 * creature.stats.metabolism;
        }

        // Increases current rest
        public override int Action()
        {
            creature.stats.currRest += creature.stats.restRecovery;
            if (creature.stats.currRest > creature.stats.maxRest) creature.stats.currRest = creature.stats.maxRest;
            return 10 * creature.stats.metabolism; // Cost of the action performed
        }
    }
}
