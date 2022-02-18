using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModularCreator : MonoBehaviour
{
    public GameObject legPrefab;
    public int numLegs;
    public float legHeight;
    public GameObject eyePrefab;
    public int numEyes;
    public int maxInLayer;
    public float spacing = 1.0f;

    void Start()
    {
        GameObject legParent = new GameObject("Leg Cluster");
        legParent.transform.position = transform.position;
        legParent.transform.parent = transform;
        GameObject go;
        float deltaAngle = 2 * Mathf.PI / numLegs;
        for (int i = 0; i < numLegs; i++)
        {
            go = Instantiate(legPrefab, Vector3.zero, Quaternion.identity, legParent.transform);
            go.transform.position = new Vector3(legParent.transform.position.x + Mathf.Cos(i * deltaAngle) * legParent.transform.localScale.x * 0.5f, transform.position.y + legHeight * transform.localScale.y, legParent.transform.position.z + Mathf.Sin(i * deltaAngle) * transform.localScale.z * 0.5f);
            go.transform.rotation = Quaternion.Euler(0, -i * deltaAngle / Mathf.PI * 180, -45);
        }
        if (numLegs % 2 == 1) legParent.transform.Rotate(new Vector3(0, 90, 0));

        GameObject eyeParent = new GameObject("Eye Cluster");
        eyeParent.transform.position = transform.position;
        eyeParent.transform.parent = transform;
        eyeParent.transform.Translate(new Vector3(0, 0.5f, 0.5f));
        float eyeDeltaAngle = 2 * Mathf.PI / Mathf.Min(numEyes, maxInLayer);
        int lap;

        for (int i = 0; i < numEyes; i++)
        {
            go = Instantiate(eyePrefab, Vector3.zero, Quaternion.identity, eyeParent.transform);
            lap = Mathf.FloorToInt(i / maxInLayer);
            go.transform.position = new Vector3(eyeParent.transform.position.x + Mathf.Cos(i * eyeDeltaAngle + (lap * eyeDeltaAngle / 2)) * ((transform.localScale.x * 0.5f) * (spacing * (1 + lap))),
                                                eyeParent.transform.position.y + Mathf.Sin(i * eyeDeltaAngle + (lap * eyeDeltaAngle / 2)) * ((transform.localScale.y * 0.5f) * (spacing * (1 + lap))),
                                                eyeParent.transform.position.z);
        }

        if (numEyes % 2 == 1) eyeParent.transform.Rotate(new Vector3(0, 0, 90));
    }
}
