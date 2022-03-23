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
        /// ID to identify an entity in the world
        /// </summary>
        public int ID { get; protected set; }

        /// <summary>
        /// Initializes the Entity
        /// </summary>
        /// <param name="ID"> ID to identify the creature </param>
        /// <param name="w"> World in which it'll reside </param>
        /// <param name="x"> X World position </param>
        /// <param name="y"> Y World position </param>
        virtual public void Init(int ID, World w, int x, int y, int hp)
        {
            this.ID = ID;
            world = w;
            this.x = x;
            this.y = y;
            maxHp = hp;
            curHp = hp;
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

        // Maximum and current health of the entity
        // TODO: esto es solo para las entidades edible
        public float maxHp { get; protected set; }
        public float curHp { get; protected set; }

        // World in which the entity resides
        [NonSerialized]
        public World world;
    }
}
