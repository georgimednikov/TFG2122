using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.Entities
{
    /// <summary>
    /// Entities that don't evolve and have simple behaviours (i.e. Plants and Corpses).
    /// Everything that it's not a Creature. They stay the same. 
    /// </summary>
    public abstract class StaticEntity : IEntity
    {
        /// <summary>
        /// Initializes the Entity
        /// </summary>
        /// <param name="w"> World in which it'll reside </param>
        /// <param name="lifeTime"> LifeTime in Ticks of the Entity </param>
        /// <param name="x"> X World position </param>
        /// <param name="y"> Y World position </param>
        protected void Init(World w, int x, int y)
        {
            world = w;
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Stable Entities tick every world tick
        /// </summary>
        virtual public void Tick()
        {

        }

        // World tile position
        public int x { get; protected set; }
        public int y { get; protected set; }
        // World in which the entity resides
        public World world { get; protected set; }
    }
}
