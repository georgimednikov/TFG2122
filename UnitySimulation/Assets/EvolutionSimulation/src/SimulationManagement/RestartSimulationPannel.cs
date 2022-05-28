using UnityEngine;
using UnityEngine.UI;

namespace UnitySimulation
{
    public class RestartSimulationPannel : MonoBehaviour
    {
        public Button restartButton;

        private void Start()
        {
            restartButton.onClick.AddListener( () => { SimulationManager.Instance.RestartSimulation(); } );
        }
    }
}
