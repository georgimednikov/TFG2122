using UnityEngine;

namespace UnitySimulation
{
    public class SimulationManager : MonoBehaviour
    {
        public int SimulationYears;
        public int InitalAnimals;
        public WorldCreaturesManager worldCreatureManager;

        UnitySimulation simulation;
        void Start()
        {
            simulation = new UnitySimulation();

            simulation.SetInitialParameters(SimulationYears, InitalAnimals);
            simulation.Init();
            simulation.Run();

            simulation.Subscribe(worldCreatureManager);
        }

        void Update()
        {
            simulation.Step();
        }
    }
}
