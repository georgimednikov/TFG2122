using UnityEngine;

namespace UnitySimulation
{
    public class SimulationManager : MonoBehaviour
    {
        //TODO: Setear estos paramatros poniendo directamente los archivos en vez de el directorio
        [Tooltip("Terrain size (in world units)")]
        public int MapSize;

        [Tooltip("Evolution years to perform before simulation")]
        public int EvolutionYears;

        [Tooltip("Number of initial species")]
        public int SpeciesNumber;

        [Tooltip("Number of individuals per species")]
        public int IndividualsNumber;

        [Tooltip("Directory where genes, chromosome, world and simulation parameters files are stored")]
        public string DataDirectory;

        [Tooltip("Directory where the species are stored after the simulation")]
        public string ExportDirectory;

        public WorldCreaturesManager worldCreatureManager;

        UnitySimulation simulation;
        void Start()
        {
            simulation = new UnitySimulation();
            simulation.SetInitialParameters(
                MapSize,
                EvolutionYears,
                SpeciesNumber,
                IndividualsNumber,
                Application.dataPath + "/" + DataDirectory + "/", 
                Application.dataPath + "/" + ExportDirectory + "/");
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
