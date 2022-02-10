namespace EvolutionSimulation.FSM.Creature.States
{
    // TODO: Estado para testear, hacer el estado correctamente
    class Moving : IState
    {
        src.Entities.Creature creature;

        public Moving(src.Entities.Creature c)
        {
            creature = c;
        }

        public bool canPerformAction(int actionPoints)
        {
            return actionPoints >= 1000 * ((200f - creature.stats.groundSpeed) / 100f);
        }

        public int Action()
        {
            int nX = 0, nY = 0;
            do
            {
                nX = creature.x + RandomGenerator.Next(-1, 2);
                nY = creature.y + RandomGenerator.Next(-1, 2);

            } while (nX == creature.x && nY == creature.y);
            if (creature.world.canMove(nX, nY))
            {
                creature.Place(nX, nY);
                return 1000 * (int)((200f - creature.stats.groundSpeed) / 100f); // Cost of the action performed
            }
            return 0;
        }
    }
}
