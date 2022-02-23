using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// When the creature has to do something but doesn't know 
    /// where he can do it, i.e. the creature wants to drink but he 
    /// doesn't know where is a lake to drink. 
    /// 
    /// It moves around the world improving his knowledge about it 
    /// (uploading his mental map of the world)
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
            return 0;
        }

        public override string ToString()
        {
            return "ExploreState";
        }
    }
}
