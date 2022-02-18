using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleLayout : MonoBehaviour
{
    public GameObject eyePrefab;
    public int numEyes;
    public int maxInLayer;
    public float spacing = 1.0f;

    void Start()
    {
        GameObject eyeparent = new GameObject();
        eyeparent.transform.parent = transform;
        GameObject go;
        float deltaAngle = 2 * Mathf.PI / Mathf.Min(numEyes, maxInLayer);
        int lap;
   
        for (int i = 0; i < numEyes; i++)
        {
            go = Instantiate(eyePrefab, Vector3.zero, Quaternion.identity, eyeparent.transform);
            lap = Mathf.FloorToInt(i / maxInLayer);
            go.transform.position = new Vector3(transform.position.x + Mathf.Cos(i * deltaAngle + (lap * deltaAngle / 2)) * ((transform.localScale.x * 0.5f) * (spacing * (1 + lap))),
                                                transform.position.y + Mathf.Sin(i * deltaAngle + (lap * deltaAngle / 2)) * ((transform.localScale.y * 0.5f) * spacing * (1 + lap)),
                                                transform.position.z);
        }
        if (numEyes % 2 == 1) eyeparent.transform.Rotate(new Vector3(0, 0, 90));
    }
}
