using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EvolutionSimulation.Unity
{
    public class SimulationManager : MonoBehaviour
    {
        public int SimulationYears;

        //CreatureManager
        UnitySimulation simulation;
        void Start()
        {
            simulation = new UnitySimulation();
            simulation.Init();
            simulation.yearsToSimulate = SimulationYears;
            simulation.Run();
        }

        void Update()
        {
            simulation.Step();
        }
    }
}
