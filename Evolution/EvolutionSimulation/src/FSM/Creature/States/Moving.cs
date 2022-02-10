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

        public bool Action()
        {
            int nX = 0, nY = 0;
            do
            {
                nX = creature.x + creature.r.Next(-1, 2);
                nY = creature.y + creature.r.Next(-1, 2);

            } while (nX != creature.x && nY != creature.y);
            if (creature.world.canMove(nX, nY))
            {
                if (creature.actionPoints < 1000 * ((200f - creature.stats.groundSpeed) / 100f)) return false;
                creature.actionPoints -= 1000 * (int)((200f - creature.stats.groundSpeed) / 100f);
                creature.Place(nX, nY);
                return true;
            }
            return false;
        }
    }
}
