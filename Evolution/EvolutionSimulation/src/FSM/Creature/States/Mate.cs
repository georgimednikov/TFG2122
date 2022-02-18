﻿namespace EvolutionSimulation.FSM.Creature.States
{
    class Mate : CreatureState
    {
        public Mate(Entities.Creature c) : base(c) { creature = c; }

        // This move is energy netural, costing the same nergy that is obtained in a tick
        public override bool canPerformAction(int actionPoints)
        {
            return actionPoints >= 1000;
        }

        // Increases current rest
        public override int Action()
        {
            Entities.Creature obj = creature.objective as Entities.Creature;
            if (obj == null) return 1000;    
            obj.ReceiveInteraction(creature, Entities.Interactions.mate);
            return 1000; // Cost of the action performed
        }

        public override string ToString()
        {
            return "AttackingState";
        }
    }
}
