using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    class Drinking : CreatureState
    {
        public Drinking(Entities.Creature c) : base(c) { creature = c; }

        public override bool canPerformAction(int actionPoints)
        {
            return actionPoints < 1000;
        }

        public override int Action()
        {
            Console.WriteLine("Drinking action");
            return 10 * creature.stats.Metabolism; // Cost of the action performed
        }

        public override string ToString()
        {
            return "DrinkingState";
        }
    }
}
