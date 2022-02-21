using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusBar : MonoBehaviour
{
    public TextMesh nameText;
    public Transform healthBar;
    
    public void SetName(string name)
    {
        nameText.text = name;
    }

    public void SetHealthBarPercentage(float percentage)
    {
        healthBar.localScale = new Vector3(percentage, healthBar.localScale.y, healthBar.localScale.z);
    }
}
