using TMPro;
using UnityEngine;

namespace UnitySimulation
{
    public class StatusBar : MonoBehaviour
    {
        public TextMeshPro nameText, statusText, statusInfo;
        public Transform healthBar;

        public void SetName(string name)
        {
            nameText.text = name;
            nameText.ForceMeshUpdate();
        }

        public void SetStatus(string status)
        {
            statusText.text = status;
            statusText.ForceMeshUpdate();
        }

        public void SetStatusInfo(string info)
        {
            statusInfo.text = info;
            statusInfo.ForceMeshUpdate();
        }

        public void SetHealthBarPercentage(float percentage)
        {
            healthBar.localScale = new Vector3(percentage, healthBar.localScale.y, healthBar.localScale.z);
        }

    }
}
