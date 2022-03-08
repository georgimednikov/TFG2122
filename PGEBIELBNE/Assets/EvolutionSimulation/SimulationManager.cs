using UnityEngine;

namespace UnitySimulation
{
    public class SimulationManager : MonoBehaviour
    {
        //TODO: Setear estos paramatros poniendo directamente los archivos en vez de el directorio
        public int SimulationYears;
        public string DataDirectory;
        public string ExportDirectory;
        public int InitalAnimals;
        public WorldCreaturesManager worldCreatureManager;

        UnitySimulation simulation;
        void Start()
        {
            simulation = new UnitySimulation();
            simulation.SetInitialParameters(
                SimulationYears, 
                Application.dataPath + "/" + DataDirectory + "/", 
                Application.dataPath + "/" + ExportDirectory + "/", 
                InitalAnimals);
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
