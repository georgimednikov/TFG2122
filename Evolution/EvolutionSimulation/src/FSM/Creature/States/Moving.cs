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

        public bool canPerformAction(int actionPoints)
        {
            return actionPoints >= 1000 * ((200f - creatura.mobility) / 100f);
        }

        public int Action()
        {
            int nX = 0, nY = 0;
            do
            {
                nX = creatura.x + RandomGenerator.Next(-1, 2);
                nY = creatura.y + RandomGenerator.Next(-1, 2);

            } while (nX == creatura.x && nY == creatura.y);
            if (creatura.world.canMove(nX, nY))
            {
                creatura.Place(nX, nY);
                return 1000 * (int)((200f - creatura.mobility) / 100f); // Cost of the action performed
            }
            return 0;
        }
    }
}
