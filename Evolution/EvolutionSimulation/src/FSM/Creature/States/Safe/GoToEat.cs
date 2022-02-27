using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    class GoToEat : CreatureState
    {
        public GoToEat(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return (int)(1000 * ((200f - creature.stats.GroundSpeed) / 100f));
        }

        public override void Action()
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
            Console.WriteLine("GoToEat action");
        }

        public override string ToString()
        {
            return "GoToEatState";
        }
    }
}
