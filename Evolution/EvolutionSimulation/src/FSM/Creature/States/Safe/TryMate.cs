namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// State going to reproduce, a male go to a female
    /// </summary>
    class TryMate : CreatureState
    {
        public TryMate(Entities.Creature c) : base(c) { creature = c; }

        // This move is energy netural, costing the same energy that is obtained in a tick
        public override bool canPerformAction(int actionPoints)
        {
            return actionPoints >= 1000;
        }

        // Increases current rest
        public override int Action()
        {
            if (creature.nearestMate == null) return 1000;
            creature.nearestMate.ReceiveInteraction(creature, Entities.Interactions.mate);
            return 1000; // Cost of the action performed
        }

        public override string ToString()
        {
            return "TryingToMateState";
        }
    }
}
