using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// When the creature has to do something but doesn't know 
    /// where it can do it, i.e. the creature wants to drink but it 
    /// doesn't know where there is a mass of water. 
    /// 
    /// It moves around the world improving its knowledge about it,
    /// updating its mental map of the world
    /// </summary>
    class Explore : CreatureState
    {
        public Explore(Entities.Creature c) : base(c) { creature = c; }

        public override bool canPerformAction(int actionPoints)
        {
            return actionPoints >= 1000 * ((200f - creature.stats.GroundSpeed) / 100f);
        }

        public override int Action()
        {
            Console.WriteLine("Explore action");

            //creature.

            return 0;
        }

        public override string ToString()
        {
            return "ExploreState";
        }
    }
}
