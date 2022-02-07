using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation
{
    /// <summary>
    /// Simulation of evolution
    /// </summary>
    public class Simulation : ISimulation
    {
        public void Init()
        {
            world = new World();
            world.Init(32);
            //Creature c = world.AddEntity<Creature>();
            //c.Init(world, 4, 4);
        }

        public void Run()
        {
            while (true)
            {
                world.Tick();
                entities.ForEach(delegate (IEntity e) { e.Tick(); });   // Orders the entity to perform a step
                creatures.Sort(new SortByMetabolism());

                delete.ForEach(delegate (IEntity e) { entities.Remove(e); });

                delete.Clear();
            }
        }

        /// <summary>
        /// Adds an entity to the list
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <returns>The added entity</returns>
        public T CreateEntity<T>() where T : IEntity, new()
        {
            T ent = new T();
            if(ent is Creature) creatures.Add(ent as Creature);
            else entities.Add(ent);
            return ent;
        }


        /// <summary>
        /// Designates an entity to be eliminated before the next frame
        /// </summary>
        public void Delete(IEntity entity)
        {
            delete.Add(entity);
        }


        // Entities in the world
        public List<IEntity> entities { get; private set; }
        public List<Creature> creatures { get; private set; }
        // Entities to be deleted
        List<IEntity> delete;

        World world;
    }

    public class SortByMetabolism : Comparer<Creature>
    {
        public override int Compare(Creature x, Creature y)
        {
            return -x.metabolism.CompareTo(y.metabolism);
        }
    }
}
