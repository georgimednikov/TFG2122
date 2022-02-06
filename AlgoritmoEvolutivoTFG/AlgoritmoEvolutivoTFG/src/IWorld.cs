using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoritmoEvolutivo
{
    interface IWorld
    {
        void Tick();

        void AddEntity(IEntity ent);
    }
}
