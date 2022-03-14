using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.Entities
{
    public class Tree : Plant
    {
        public static float movementPenalty { get; private set; } = UniverseParametersManager.parameters.treeMovementPenalty;
        public Tree()
        {
            type = PlantType.Tree;
        }

    }
}
