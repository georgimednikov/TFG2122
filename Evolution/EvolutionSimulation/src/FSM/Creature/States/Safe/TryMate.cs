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
            Console.WriteLine("Try to mate");

            if (creature.GetClosestPossibleMatePosition() != null &&
                creature.DistanceToObjective(creature.GetClosestPossibleMatePosition()) <= 1) //TODO quitar este 1
                creature.GetClosestPossibleMate().ReceiveInteraction(creature, Entities.Interactions.mate);
        }

        public override string ToString()
        {
            return "TryingToMateState";
        }
    }
}
