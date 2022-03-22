using System;

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
            return UniverseParametersManager.parameters.baseActionCost;//TODO
        }

        // Increases current rest
        public override void Action()
        {
            if (creature.GetClosestPossibleMatePosition() != null &&
                creature.DistanceToObjective(creature.GetClosestPossibleMatePosition()) <= UniverseParametersManager.parameters.adjacentLength)
                creature.GetClosestPossibleMate().ReceiveInteraction(creature, Entities.Interactions.mate);
            Console.WriteLine(creature.speciesName + " TRIES TO MATE");
        }

        public override string ToString()
        {
            return "TryingToMateState";
        }

        /// <summary>
        /// Text used to display state in simulation
        /// </summary>
        public override string GetInfo()
        {
            return "TRYING TO MATE";
        }
    }
}
