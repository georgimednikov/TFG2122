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

        public float TimeBetweenSteps = 1f;

        //[Tooltip("Directory where genes, chromosome, world and simulation parameters files are stored")]
        //public string DataDirectory;

        //[Tooltip("Directory where the species are stored after the simulation")]
        //public string ExportDirectory;

        public WorldCreaturesManager worldCreatureManager;
        public WorldCorpseManager worldCorpseManager;
        public GenerateWorld worldGenerator;

        UnitySimulation simulation;

        static public SimulationManager Instance { get => _instance; }
        static SimulationManager _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                _instance.EvolutionYears = EvolutionYears;
                _instance.SpeciesNumber = SpeciesNumber;
                _instance.IndividualsNumber = IndividualsNumber;
                _instance.UniverseParameters = UniverseParameters;
                _instance.Chromosome = Chromosome;
                _instance.AbilityUnlocks = AbilityUnlocks;
                _instance.GeneSimilarity = GeneSimilarity;
                _instance.WorldMap = WorldMap;
                _instance.RegionMap = RegionMap;
                _instance.worldCorpseManager = worldCorpseManager;
                _instance.worldCreatureManager = worldCreatureManager;
                _instance.worldGenerator = worldGenerator;

                Destroy(gameObject);
            }
        }
        void Start()
        {
            if (worldGenerator == null)
                Debug.LogError("WorldGenerator is not assigned");
            if (worldCreatureManager == null)
                Debug.LogError("WorldCreatureManager is not assigned");
            if (worldCorpseManager == null)
                Debug.LogError("WorldCorpseManager is not assigned");
            
           

            string universeFileRaw = UniverseParameters == null ? null : UniverseParameters.text;
            string chromosomeFileRaw = Chromosome == null ? null : Chromosome.text;
            string abilitiesFileRaw = AbilityUnlocks == null ? null : AbilityUnlocks.text;
            string geneFileRaw = GeneSimilarity == null ? null : GeneSimilarity.text;
            string worldFileRaw = WorldMap == null ? null : WorldMap.text;
            string regionFileRaw = RegionMap == null ? null : RegionMap.text;
           
            simulation = new UnitySimulation();
            simulation.InitTracker();
            simulation.GenerateWorld = worldGenerator;
            simulation.Init(
                EvolutionYears, SpeciesNumber, IndividualsNumber,
                universeFileRaw,
                chromosomeFileRaw, abilitiesFileRaw,
                geneFileRaw,
                worldFileRaw, regionFileRaw,
                Application.dataPath + "/Export/"
                );

            System.Timers.Timer timer = new System.Timers.Timer(5000);
            // TODO: Se puede hacer que sea el propio tracker el que haga el flush automatico cada x tiempo
            timer.Elapsed += (o, args) => { Tracker.Instance.Flush(); };
            timer.AutoReset = true;
            timer.Start();

            simulation.Run();

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

        public float GetTimeBetweenSteps()
        {
            return TimeBetweenSteps;
        }
        public int GetCurrentTicks()
        {
            return simulation.GetCurrentTicks();
        }
        public int GetTicksInDay()
        {
            return simulation.GetTicksInDay();
        }

        void OnDestroy()
        {
            simulation.EndTracker();
        }
    }
}
