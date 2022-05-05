using System.Collections;
using UnityEngine;
using Telemetry;
using Telemetry.Events;
namespace UnitySimulation
{
    public class SimulationManager : MonoBehaviour
    {
        //TODO: Setear estos paramatros poniendo directamente los archivos en vez de el directorio
        [Tooltip("Evolution years to perform before simulation")]
        public int EvolutionYears;
        public bool chromosome;

        [Tooltip("Number of initial species")]
        public int SpeciesNumber;

        [Tooltip("Number of individuals per species")]
        public int IndividualsNumber;

        public TextAsset UniverseParameters;
        public TextAsset Chromosome;
        public TextAsset AbilityUnlocks;
        public TextAsset GeneSimilarity;
        public TextAsset WorldMap;
        public TextAsset RegionMap;
        [SerializeField]
        string SpeciesDir = "/ProgramData/InitialSpecies/";

        public static float TimeBetweenSteps = 1f;

        //[Tooltip("Directory where genes, chromosome, world and simulation parameters files are stored")]
        //public string DataDirectory;

        //[Tooltip("Directory where the species are stored after the simulation")]
        //public string ExportDirectory;

        public WorldCreaturesManager worldCreatureManager;
        public WorldCorpseManager worldCorpseManager;
        public GenerateWorld worldGenerator;

        UnitySimulation simulation;
        void Start()
        {
            if (worldGenerator == null)
                Debug.LogError("WorldGenerator is not assigned");
            if (worldCreatureManager == null)
                Debug.LogError("WorldCreatureManager is not assigned");
            if (worldCorpseManager == null)
                Debug.LogError("WorldCorpseManager is not assigned");
            
            Tracker.Instance.Init();
            Tracker.Instance.Track(new SessionStart());

            string universeFileRaw = UniverseParameters == null ? null : UniverseParameters.text;
            string chromosomeFileRaw = Chromosome == null ? null : Chromosome.text;
            string abilitiesFileRaw = AbilityUnlocks == null ? null : AbilityUnlocks.text;
            string geneFileRaw = GeneSimilarity == null ? null : GeneSimilarity.text;
            string worldFileRaw = WorldMap == null ? null : WorldMap.text;
            string regionFileRaw = RegionMap == null ? null : RegionMap.text;
           
            simulation = new UnitySimulation();
            simulation.generateWorld = worldGenerator;
            simulation.Init(
                EvolutionYears, SpeciesNumber, IndividualsNumber,
                universeFileRaw,
                chromosomeFileRaw, abilitiesFileRaw,
                geneFileRaw,
                worldFileRaw, regionFileRaw,
                Application.dataPath + "/Export/");

            if (chromosome) simulation.Run("");
            else simulation.Run(Application.dataPath + SpeciesDir);

            simulation.Subscribe(worldCreatureManager);
            simulation.Subscribe(worldCorpseManager);
            StartCoroutine(StepWorld());
        }

        IEnumerator StepWorld()
        {
            while (true)
            {
                simulation.SimulateStep();
                yield return new WaitForSeconds(TimeBetweenSteps);
                
                //worldCreatureManager.AfterCorroutine();
            }
        }

        public static float GetTimeBetweenSteps()
        {
            return TimeBetweenSteps;
        }

        void OnDestroy()
        {
            Tracker.Instance.Track(new SessionEnd());
            Tracker.Instance.Flush();
        }
    }
}
