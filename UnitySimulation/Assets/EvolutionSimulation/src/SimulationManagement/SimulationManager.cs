using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace UnitySimulation
{
    /// <summary>
    /// Singleton to manage the configuration, the simulation and the other managers 
    /// </summary>
    public class SimulationManager : MonoBehaviour
    {
        // Singleton
        static public SimulationManager Instance { get => _instance; }
        static SimulationManager _instance;

        [Tooltip("Expected time to pass between every simulation tick")]
        public float TimeBetweenSteps = 1f;

        [Tooltip("Evolution years to perform before simulation")]
        public int EvolutionYears;

        [Tooltip("Number of initial species")]
        public int SpeciesNumber;

        [Tooltip("Number of individuals per species")]
        public int IndividualsNumber;

        [Tooltip("Universe Parameters file")]
        public TextAsset UniverseParameters;

        [Tooltip("Chromosome file")]
        public TextAsset Chromosome;

        [Tooltip("Ability Unlocks file")]
        public TextAsset AbilityUnlocks;

        [Tooltip("Gene Similarity file")]
        public TextAsset GeneSimilarity;

        [Tooltip("World map file")]
        public TextAsset WorldMap;

        [Tooltip("Region map file")]
        public TextAsset RegionMap;

        // Managers
        public WorldCreaturesManager worldCreatureManager;
        public WorldCorpseManager worldCorpseManager;
        public WorldGenerator worldGenerator;

        /// <summary>
        /// UI Pannel to restart the simulation
        /// </summary>
        public GameObject restartPannel;

        /// <summary>
        /// The simulation system adapted to Unity
        /// </summary>
        UnitySimulation simulation;

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
            
            simulation = new UnitySimulation();
            StartSimulation();
        }

        public void RestartSimulation()
        {
            worldCreatureManager.Restart();
            worldCorpseManager.Restart();
            restartPannel.SetActive(false);
            StartSimulation();
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

        /// <summary>
        /// Coroutine where the simulation steps are performed,
        /// and it is executed every 'TimeBetweenSteps'
        /// </summary>
        IEnumerator StepWorld()
        {
            //Debug.Log(simulation.SimulationEnd);
            while (!simulation.SimulationEnd)
            {
                // Simulate an step
                simulation.SimulateStep();
                // Update every manager
                worldCreatureManager.StepUpdate(simulation.World);
                worldCorpseManager.StepUpdate(simulation.World);
                yield return new WaitForSeconds(TimeBetweenSteps);
            }
            restartPannel.SetActive(true);
            yield return null;
        }

        void StartSimulation()
        {
            string universeFileRaw = UniverseParameters == null ? null : UniverseParameters.text;
            string chromosomeFileRaw = Chromosome == null ? null : Chromosome.text;
            string abilitiesFileRaw = AbilityUnlocks == null ? null : AbilityUnlocks.text;
            string geneFileRaw = GeneSimilarity == null ? null : GeneSimilarity.text;
            string worldFileRaw = WorldMap == null ? null : WorldMap.text;
            string regionFileRaw = RegionMap == null ? null : RegionMap.text;

            simulation.Init(
                EvolutionYears, SpeciesNumber, IndividualsNumber, Application.dataPath + "/Export/",  // the data is not actually exported
                null,
                universeFileRaw,
                chromosomeFileRaw, abilitiesFileRaw,
                geneFileRaw,
                worldFileRaw, regionFileRaw
                );
            simulation.Run();

            GenerateMap();
            StartCoroutine(StepWorld());
        }

        void GenerateMap()
        {
            worldGenerator.SetWorld(simulation.World);
            worldGenerator.MapGen();
        }
    }
}
