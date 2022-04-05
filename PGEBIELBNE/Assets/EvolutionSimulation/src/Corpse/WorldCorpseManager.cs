using System.Collections;
using System.Collections.Generic;
using EvolutionSimulation;
using EvolutionSimulation.Entities;
using EvolutionSimulation.Utils;
using UnityEngine;

public class WorldCorpseManager : MonoBehaviour, IListener<World>
{
    public GameObject corpsePrefab;

    Dictionary<int, GameObject> _corpses = new Dictionary<int, GameObject>();

    public void OnNotify(World info)
    {
        List<int> currCorpses = new List<int>();
        
        foreach(KeyValuePair<int, StaticEntity> se in info.StaticEntities)
        {
            if (se.Value is Plant) continue;
            currCorpses.Add(se.Key);
        }
        
        CheckCorpses(info, currCorpses);
    }

    void CheckCorpses(World w, List<int> corpses)
    {
        //Debug.Log("ManagedCorpses: " + _corpses.Count);
        //Debug.Log("WorldCorpses: " + corpses.Count);

        // Check if a corpse has despawned
        foreach (int c in _corpses.Keys)
        {
            if(w.GetStaticEntity(c) == null)
            {
                Destroy(_corpses[c]);
                _corpses.Remove(c);
            }
        }

        // Check if a new corpse has spawned
        foreach (int c in corpses)
        {
            if (!_corpses.ContainsKey(c))
            {
                // Instantiate corpse
                _corpses.Add(c, SpawnCorpse(w.GetStaticEntity(c) as Corpse));
            }
        }
    }


    GameObject SpawnCorpse(Corpse c)
    {
        Terrain terrain = GetComponent<Terrain>();

        RaycastHit hit;
        // Position in Y axis + 10 (no specific reason for that number) => No point in the world will be higher.  
        Physics.Raycast(new Vector3(c.x, terrain.terrainData.size.y + 10, c.y), Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("World"));
        GameObject gO = Instantiate(corpsePrefab, new Vector3(hit.point.x, hit.point.y, hit.point.z), Quaternion.identity);
        return gO;
    }
}
