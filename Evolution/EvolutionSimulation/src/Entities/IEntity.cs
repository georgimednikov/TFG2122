using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation
{
    public interface IEntity
    {
        void Tick();

        int x { get; }

        int y { get; }
    }
}
