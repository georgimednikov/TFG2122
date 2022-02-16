namespace EvolutionSimulation.FSM.Creature.States
{
    class Attacking : CreatureState
    {
        public Attacking(Entities.Creature c) : base(c) { creature = c; }

        // This move is energy netural, costing the same nergy that is obtained in a tick
        public override bool canPerformAction(int actionPoints)
        {
            return actionPoints >= 1000;
        }

        // Increases current rest
        public override int Action()
        {
            if (creature.objective == null) return 1000;    
            // TODO: ahora mismo si matas a la creatura que ataques te quedas atacando a la nada, 
            // Poner transicion de attacking a iddle bien
            Entities.Creature obj = (creature.objective as Entities.Creature);
            obj.ReceiveInteraction(creature, Entities.Interactions.attack);
            return 1000; // Cost of the action performed
        }

        public override string ToString()
        {
            return "AttackingState";
        }
    }
}
