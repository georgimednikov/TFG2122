using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// Hide from an enemy
    /// </summary>
    class Hide : CreatureState
    {
        public Hide(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return (int)(UniverseParametersManager.parameters.baseActionCost * 
            ((creature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Mobility) - creature.stats.GroundSpeed) / 
            (creature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Mobility) / 2)));
        }

        public override void Action()    // TODO: Que es esconderse?
        {
            Console.WriteLine(creature.speciesName + " HIDES");
        }

        public override string ToString()
        {
            return "HideState";
        }
    }
}
