namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// State going to reproduce, a male go to a female
    /// </summary>
    class TryMate : CreatureState
    {
        public TryMate(Entities.Creature c) : base(c) { creature = c; }

        // This move is energy netural, costing the same energy that is obtained in a tick
        public override int GetCost()
        {
            return 1000;
        }

        // Increases current rest
        public override void Action()
        {
            if (creature.nearestMate != null)
                creature.nearestMate.ReceiveInteraction(creature, Entities.Interactions.mate);
        }

        public override string ToString()
        {
            return "TryingToMateState";
        }
    }
}
