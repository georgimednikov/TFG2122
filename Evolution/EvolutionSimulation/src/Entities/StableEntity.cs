using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.Entities
{
    /// <summary>
    /// Entities that don't evolve and have simple behaviours (i.e. Plants and Corpses).
    /// Everything that it's not a Creature. They stay the same during all its LifeTime. 
    /// </summary>
    public abstract class StableEntity : IEntity
    {
        /// <summary>
        /// Initializes the Entity
        /// </summary>
        /// <param name="w"> World in which it'll reside </param>
        /// <param name="lifeTime"> LifeTime in Ticks of the Entity </param>
        /// <param name="x"> X World position </param>
        /// <param name="y"> Y World position </param>
        protected void Init(World w, int lifeTime, int x, int y)
        {
            world = w;
            this.lifeTime = lifeTime;
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Stable Entities progressively die every tick
        /// </summary>
        public void Tick()
        {
            lifeTime--;
            Update();
        }

        // Abstract Update function called every tick
        // LifeTime its reduced every Updated call
        protected abstract void Update();

        // World tile position
        public int x { get; protected set; }
        public int y { get; protected set; }
        // World in which the entity resides
        public World world { get; protected set; }
        // Remaining life time of this entity
        public int lifeTime { get; protected set; }
    }
}
