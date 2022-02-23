using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    class GoToMate : CreatureState
    {
        public GoToMate(Entities.Creature c) : base(c) { creature = c; }

        public override bool canPerformAction(int actionPoints)
        {
            return actionPoints >= 1000 * ((200f - creature.stats.GroundSpeed) / 100f);
        }

        public override int Action()
        {
            //int nX = creature.objective.x - creature.x,
            //    nY = creature.objective.y - creature.y;
            //nX = nX > 0 ? 1 : (nX < 0 ? -1 : 0);
            //nY = nY > 0 ? 1 : (nY < 0 ? -1 : 0);
            //// TODO: A-estrella ?

            //if (creature.world.canMove(nX, nY)) // TODO: ahora mismo si encuentra un obstaculo no hace nada
            //{
            //    creature.Place(nX, nY);
            //    return (int)(1000 * ((200f - creature.stats.GroundSpeed) / 100f)); // Cost of the action performed
            //}
            Console.WriteLine("GoToMate action");

            return 0;
        }

        public override string ToString()
        {
            return "GoToMateState";
        }
    }
}
