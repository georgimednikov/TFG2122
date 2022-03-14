using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimulation
{
    public class StatusBar : MonoBehaviour
    {
        public TextMesh nameText, statusText, statusInfo;
        public Transform healthBar;

        public void SetName(string name)
        {
            nameText.text = name;
        }

        public void SetStatus(string status)
        {
            statusText.text = status;
        }

        public void SetStatusInfo(string info)
        {
            statusInfo.text = info;
        }

        public void SetHealthBarPercentage(float percentage)
        {
            healthBar.localScale = new Vector3(percentage, healthBar.localScale.y, healthBar.localScale.z);
        }
    }
}
