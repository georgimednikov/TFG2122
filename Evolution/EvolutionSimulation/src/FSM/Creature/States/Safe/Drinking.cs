using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    class Drinking : CreatureState
    {
        public Drinking(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return UniverseParametersManager.parameters.drinkingCostMultiplier * creature.stats.Metabolism;
        }

        public override void Action()
        {
            Console.WriteLine("Drinking action");
        }

        public override string ToString()
        {
            return "DrinkingState";
        }
    }
}
