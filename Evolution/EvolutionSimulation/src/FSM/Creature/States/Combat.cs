using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    // TODO: Estado para testear, hacer el estado correctamente

    class Combat : CreatureState
    {
        public Combat(Entities.Creature c) : base(c) { creature = c; }

        public override bool canPerformAction(int actionPoints)
        {
            return actionPoints < 1000;
        }

        public override int Action()
        {
            Console.WriteLine("Combat Action");
            return 1000;
        }

        public override string ToString()
        {
            return "CombatState";
        }
    }
}
