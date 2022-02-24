using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    // TODO: Estado para testear, hacer el estado correctamente

    class Safe : CreatureState
    {
        public Safe(Entities.Creature c) : base(c) { creature = c; }

        public override bool canPerformAction(int actionPoints)
        {
            return actionPoints >= 1000;
        }

        public override int Action()
        {
            Console.WriteLine("Safe Action");
            return 1000;
        }

        public override string ToString()
        {
            return "SafeState";
        }
    }
}
