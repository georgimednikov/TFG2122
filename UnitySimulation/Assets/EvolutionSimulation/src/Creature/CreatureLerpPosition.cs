using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureLerpPosition : MonoBehaviour
{
    public float lerpTime = 1;

    bool lerping = false;
    float lerpPercentage = 0;
    Vector3 startLerpPos, lerpPos;

    void Update()
    {
        if (lerping)
        {
            lerpPercentage += Time.deltaTime / UnitySimulation.SimulationManager.Instance.GetTimeBetweenSteps();
            if (lerpPercentage >= 1)
            {
                lerping = false;
                lerpPercentage = 1;
            }
            transform.position = Vector3.Lerp(startLerpPos, lerpPos, lerpPercentage);
        }
    }

    public void LerpToPosition(Vector3 pos)
    {
        startLerpPos = transform.position;
        lerpPos = pos;
        lerping = true;
        lerpPercentage = 0;
    }
}
