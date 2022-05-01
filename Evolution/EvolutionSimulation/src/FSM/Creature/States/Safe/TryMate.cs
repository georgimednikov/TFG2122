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
        int mateID; Vector3Int matePos;

        // This move is energy netural, costing the same energy that is obtained in a tick
        public override int GetCost()
        {
            return UniverseParametersManager.parameters.baseActionCost;
        }

        // Increases current rest
        public override void OnEntry()
        {
            base.OnEntry();
            if (creature.Mate(out mateID, out matePos) &&
                creature.DistanceToObjective(matePos) <= UniverseParametersManager.parameters.adjacentLength)
                creature.world.GetCreature(mateID).ReceiveInteraction(creature, Entities.Interactions.mate);
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
            return creature.speciesName + " with ID: " + creature.ID + " TRIES TO MATE WITH ID: " + creature.matingCreature + " AT (" + matePos.x + ", " + matePos.y + ")";
        }
    }
}
