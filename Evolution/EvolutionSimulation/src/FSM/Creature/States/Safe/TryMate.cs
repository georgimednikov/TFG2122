namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// State trying to reproduce. It is only for males.
    /// A male interact with a female and "ask" to mate.
    /// </summary>
    class TryMate : CreatureState
    {
        public TryMate(Entities.Creature c) : base(c) { creature = c; }

        // This move is energy netural, costing the same energy that is obtained in a tick
        public override int GetCost()
        {
            return (int)UniverseParametersManager.parameters.baseActionCost;//TODO
        }

        // Increases current rest
        public override void Action()
        {
            if (creature.GetClosestPossibleMate() != null)
                creature.GetClosestPossibleMate().ReceiveInteraction(creature, Entities.Interactions.mate);
        }

        public override string ToString()
        {
            return "TryingToMateState";
        }
    }
}
