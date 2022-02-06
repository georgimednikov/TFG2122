using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoritmoEvolutivo
{
    public class Simulation
    {
        public void Init()
        {

        }

        public T AddEntity<T>() where T : IEntity, new()
        {
            T ent = new T();
            entities.Add(ent);
            world.AddEntity(ent);
            return ent;
        }

        //public IEntity GetEntity()
        //{

        //}

        public void Delete(IEntity entity)
        {
            delete.Add(entity);
        }

        public void Run()
        {
            while (true)
            {
                world.Tick();

                entities.ForEach(delegate (IEntity e) { e.Tick(); });

                delete.ForEach(delegate (IEntity e) { entities.Remove(e); });

                delete.Clear();
            }
        }

        public List<IEntity> entities { get; private set; }
        List<IEntity> delete;
        IWorld world;
    }
}
