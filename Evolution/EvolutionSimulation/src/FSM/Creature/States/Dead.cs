using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.States
{
    // TODO: Estado para testear, hacer el estado correctamente
    class Dead : CreatureState
    {
        public Dead(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return Math.Max(creature.ActionPoints, 1); // This allows for one death and prevents subsequent ones
        }

        /// <summary>
        /// If the creature dies, it consumes all its action points to avoid doing actions while dead. 
        /// Creates a corpse in the death position.
        /// </summary>
        public override void Action()
        {
            Console.WriteLine(creature.speciesName + " DIES");
            creature.world.Destroy(creature);
            Entities.Corpse corpse = creature.world.CreateStableEntity<Entities.Corpse>();
            corpse.Init(creature.world,
                World.ticksHour + ((creature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Size) * World.ticksHour * World.hoursDay * 7) - World.ticksHour) 
                * (creature.stats.Size / creature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Size)),  // From an hour to two weeks of lifetime, depending on size
                creature.x, creature.y, 
                creature.stats.MaxHealth * UniverseParametersManager.parameters.rotStartMultiplier,    // The less health, the faster the rot
                creature.HasAbility(Genetics.CreatureFeature.Venomous, Genetics.CreatureChromosome.AbilityUnlock[Genetics.CreatureFeature.Venomous]) ?  
                (creature.stats.Venom / creature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Venomous)) : 0,   // If it is venomous it will be more risky to eat 
                (int)(creature.stats.Venom),
                creature.stats.Venom * 0.25f, 
                UniverseParametersManager.parameters.corpseNutritionPointsMultiplier * creature.stats.Size / creature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Size),  // TODO: testiar
                creature.stats.MaxHealth);  // TODO: poner que sea un porcentaje de esta vida
        }

        public override string ToString()
        {
            return "DeadState";
        }
    }
}
