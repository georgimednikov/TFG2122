using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.States
{
    // TODO: Estado para testear, hacer el estado correctamente
    class Moving : IState
    {
        EvolutionSimulation.Creature creatura;

        public Moving(EvolutionSimulation.Creature c)
        {
            creatura = c;
        }

        public bool Action()
        {
            int nX = 0, nY = 0;
            do
            {
                nX = creatura.x + creatura.r.Next(-1, 2);
                nY = creatura.y + creatura.r.Next(-1, 2);

            } while (nX != creatura.x && nY != creatura.y);
            if (creatura.world.canMove(nX, nY))
            {
                if (creatura.actionPoints < 1000 * ((200f - creatura.mobility) / 100f)) return false;
                creatura.actionPoints -= 1000 * (int)((200f - creatura.mobility) / 100f);
                creatura.Place(nX, nY);
                return true;
            }
            return false;
        }
    }
}
