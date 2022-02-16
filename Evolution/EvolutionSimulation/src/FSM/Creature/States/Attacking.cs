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
            Entities.Creature obj = (creature.objective as Entities.Creature);
            obj.ReceiveInteraction(creature, Entities.Interactions.attack);
            return 1000; // Cost of the action performed
        }
    }
}
