using System;

namespace EvolutionSimulation.Entities
{
    public class EdiblePlant : Plant
    {
        public bool eaten { get; protected set; } = false;
        protected int regrowthTime;
        protected float nutritionalValue;
        override public bool Tick()
        {

            if (eaten)  // If it is eaten, it remains so until it is fully grown back
                eaten = curHp < maxHp;

            // It regrows steadily even while not fully eaten
            curHp = Math.Min(curHp + (1 / (float)regrowthTime) * maxHp, maxHp);

            return !eaten;
        }

        /// <summary>
        /// if the plant has been eaten, it doesn't give nutritional value
        /// </summary>
        /// <returns> Return the nutritional value</returns>
        public void ReceiveInteraction(Creature other, Interactions type)
        {
            if (type != Interactions.eat || eaten) 
                return;
            world.StaticEntitiesToUpdate.Add(this);

            float dealt = Math.Min(other.stats.Damage, curHp);

            // Food effectiveness is (normally) reduced for omnivores since they can consume all sources of nutritients, so they are worse at it.
            float nutritionalEffectiveness = 1.0f;
            if (other.stats.Diet == Genetics.Diet.Omnivore) nutritionalEffectiveness = UniverseParametersManager.parameters.omnivorousNutritionMultiplier;

            other.stats.CurrEnergy += ((dealt / maxHp) * nutritionalValue) * nutritionalEffectiveness;
            curHp -= dealt;

            if(curHp <= 0) {
                eaten = true;
#if TRACKER_ENABLED
                Telemetry.Tracker.Instance.Track(new Telemetry.Events.PlantEaten(world.tick, ID, x, y));
#endif
            }
        }
    }
}
