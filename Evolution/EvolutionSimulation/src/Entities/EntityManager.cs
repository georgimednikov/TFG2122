using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation
{
    public sealed class EntityManager
    {
        /// <summary>
        /// Adds an entity to the list
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <returns>The added entity</returns>
        public T CreateEntity<T>() where T : IEntity, new()
        {
            T ent = new T();
            entities.Add(ent);
            return ent;
        }


        /// <summary>
        /// Designates an entity to be eliminated before the next frame
        /// </summary>
        public void Delete(IEntity entity)
        {
            delete.Add(entity);
        }

        // Singleton stuff
        static readonly EntityManager _instance = new EntityManager();
        private EntityManager() { }
        public static EntityManager Instance { get { return _instance; } }
        
        // Attributes
        // Entities in the world
        public List<IEntity> entities { get; private set; }
        // Entities to be deleted in the world
        List<IEntity> delete;

    }
}
