namespace EvolutionSimulation.FSM.Creature.States
{
    // TODO: Estado para testear, hacer el estado correctamente
    class Moving : CreatureState
    {
        public Moving(Entities.Creature c) : base(c) { creature = c; }

        public override bool canPerformAction(int actionPoints)
        {
            return actionPoints >= 1000 * ((20f - creature.stats.groundSpeed) / 10f);
        }

        public override int Action()
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
                return (int)(1000 * ((20f - creature.stats.groundSpeed) / 10f)); // Cost of the action performed
            }
            return 0;
        }
    }
}
