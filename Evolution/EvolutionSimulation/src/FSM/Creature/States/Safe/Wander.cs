using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// Move randomly until the creature has something to do
    /// </summary>
    class Wander : CreatureState
    {
        public Wander(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return (int)(UniverseParametersManager.parameters.baseActionCost * 
                ((creature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Mobility) - creature.stats.GroundSpeed) / 
                (creature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Mobility) / 2f)));
        }

        public override void Action()
        {
            int nX, nY;
            do
            {
                nX = creature.x + RandomGenerator.Next(-1, 2);
                nY = creature.y + RandomGenerator.Next(-1, 2);

            } while (nX == creature.x && nY == creature.y);
            if (creature.world.CanMove(nX, nY) && creature.CheckTemperature(nX,nY))
            {
                creature.Place(nX, nY);
            }
        }

        public override string ToString()
        {
            return "WanderState";
        }

        /// <summary>
        /// Text used to display state in simulation
        /// </summary>
        public override string GetInfo()
        {
            return creature.speciesName + " with ID: " + creature.ID + " WANDERS (" + creature.x + ", " + creature.y + ")";
        }
    }
}
