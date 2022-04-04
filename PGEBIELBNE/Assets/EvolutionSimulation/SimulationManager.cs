using System.Collections;
using UnityEngine;

namespace UnitySimulation
{
    public class SimulationManager : MonoBehaviour
    {
        //TODO: Setear estos paramatros poniendo directamente los archivos en vez de el directorio
        [Tooltip("Evolution years to perform before simulation")]
        public int EvolutionYears;

        [Tooltip("Number of initial species")]
        public int SpeciesNumber;

        [Tooltip("Number of individuals per species")]
        public int IndividualsNumber;

        public TextAsset UniverseParameters;
        public TextAsset Chromosome;
        public TextAsset AbilityUnlocks;
        public TextAsset GeneSimilarity;
        public TextAsset SpeciesSimilarity;
        public TextAsset WorldConfig;


        public float TimeBetweenSteps = 1;

        //[Tooltip("Directory where genes, chromosome, world and simulation parameters files are stored")]
        //public string DataDirectory;

        //[Tooltip("Directory where the species are stored after the simulation")]
        //public string ExportDirectory;

        public WorldCreaturesManager worldCreatureManager;
        public GenerateWorld worldGenerator;

        UnitySimulation simulation;
        void Start()
        {
            if (worldGenerator == null)
                Debug.LogError("WorldGenerator is not assigned");
            simulation = new UnitySimulation();
            simulation.generateWorld = worldGenerator;
            simulation.Init(
                EvolutionYears, SpeciesNumber, IndividualsNumber,
                UniverseParameters.text,
                Chromosome.text, AbilityUnlocks.text,
                GeneSimilarity.text, SpeciesSimilarity.text,
                WorldConfig.text,
                Application.dataPath + "/Export/"
                );
            
            simulation.Run();

            simulation.Subscribe(worldCreatureManager);
            StartCoroutine(StepWorld());
        }

        IEnumerator StepWorld()
        {
            while (true)
            {
                simulation.Step();
                yield return new WaitForSeconds(TimeBetweenSteps);
            }
        }
    }
}
