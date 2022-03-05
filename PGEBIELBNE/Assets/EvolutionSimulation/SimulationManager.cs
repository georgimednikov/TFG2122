using UnityEngine;

namespace UnitySimulation
{
    public class SimulationManager : MonoBehaviour
    {
        public int SimulationYears;
        public WorldCreaturesManager creatureManager;

        UnitySimulation simulation;
        void Start()
        {
            simulation = new UnitySimulation();

            simulation.yearsToSimulate = SimulationYears;
            simulation.Init();
            simulation.Run();

            simulation.Subscribe(creatureManager);
        }

        void Update()
        {
            simulation.Step();
        }
    }
}
