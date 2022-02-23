using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    // TODO: Estado para testear, hacer el estado correctamente
    class ChaseEnemy : CreatureState
    {
        public ChaseEnemy(Entities.Creature c) : base(c) { creature = c; }

        public override bool canPerformAction(int actionPoints)
        {
            return actionPoints >= 1000 * ((200f - creature.stats.GroundSpeed) / 100f);
        }

        public override int Action()
        {
            Console.WriteLine("Explore action");
            return 0;
        }

        public override string ToString()
        {
            return "ChaseEnemyState";
        }
    }
}
