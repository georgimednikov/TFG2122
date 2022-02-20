using System;
using UnityEngine;

namespace EvolutionSimulation.Unity
{
    public class InterpretJSON : MonoBehaviour
    {
        public GameObject body;
        public GameObject leg;
        public GameObject[] mouths;
        public GameObject[] armours;
        public GameObject spikes;
        public GameObject hair;
        public GameObject beard;
        public GameObject statusBar;

        // Start is called before the first frame update
        public void CreateCreature(TextAsset json)
        {
            SpeciesExport creature = JsonUtility.FromJson<SpeciesExport>(json.text);

            InstantiateBody(creature);

            InstantiateLegs(creature);

            InstantiateMouth(creature);

            InstantiateArmour(creature);

            InstantiateSpikes(creature);

            // InstantiateHair(creature);

            InstantiateBeard(creature);

            InstantiateStatusBar(creature);
        }

        private void InstantiateMouth(SpeciesExport creature)
        {
            GameObject go = Instantiate(mouths[(int)creature.stats.Diet], transform);
            go.transform.localScale = Vector3.one * creature.stats.ModifyStatByAge(creature.stats.Size) / 100f;
            go.transform.localPosition = Vector3.up * creature.stats.ModifyStatByAge(creature.stats.Size) / 100f * 1.5f;
        }

        private void InstantiateArmour(SpeciesExport creature)
        {
            GameObject go;
            if (creature.stats.Armor >= 15)
                go = Instantiate(armours[1], transform);
            else if (creature.stats.Armor >= 8)
                go = Instantiate(armours[0], transform);
            else return; // If Armor is low, don't put on armor
            go.transform.localScale = Vector3.one * creature.stats.ModifyStatByAge(creature.stats.Size) / 100f;
            go.transform.localPosition = Vector3.up * creature.stats.ModifyStatByAge(creature.stats.Size) / 100f;
        }

        private void InstantiateBody(SpeciesExport creature)
        {
            GameObject go = Instantiate(body, transform);
            go.transform.localScale = Vector3.one * creature.stats.ModifyStatByAge(creature.stats.Size) / 100f;
            go.transform.localPosition = Vector3.up * creature.stats.ModifyStatByAge(creature.stats.Size) / 100f;
        }

        private void InstantiateLegs(SpeciesExport creature)
        {
            GameObject go;
            GameObject legParent = new GameObject("Leg Cluster");
            legParent.transform.parent = transform;
            legParent.transform.localPosition = Vector3.up * creature.stats.ModifyStatByAge(creature.stats.Size) / 100f;
            legParent.transform.localScale = Vector3.one * creature.stats.ModifyStatByAge(creature.stats.Size) / 100f;

            float deltaAngle = 2 * Mathf.PI / creature.stats.Members;
            for (int i = 0; i < creature.stats.Members; i++)
            {
                go = Instantiate(leg, legParent.transform);
                go.transform.position = new Vector3(legParent.transform.position.x + Mathf.Cos(i * deltaAngle) * legParent.transform.localScale.x * 0.5f,
                    0.5f * legParent.transform.localScale.y,
                    legParent.transform.position.z + Mathf.Sin(i * deltaAngle) * legParent.transform.localScale.z * 0.5f);
                go.transform.rotation = Quaternion.Euler(0, -i * deltaAngle / Mathf.PI * 180, -45);
            }
            if (creature.stats.Members % 2 == 1) legParent.transform.Rotate(new Vector3(0, 90, 0));
        }

        private void InstantiateSpikes(SpeciesExport creature)
        {
            if (creature.stats.Counter == -1) return; // TODO: how to check if the creture has this ability?
            GameObject go = Instantiate(spikes, transform);
            go.transform.localScale = Vector3.one * creature.stats.ModifyStatByAge(creature.stats.Size) / 100f;
            go.transform.localPosition = Vector3.up * 1.5f * creature.stats.ModifyStatByAge(creature.stats.Size) / 100f;
        }

        private void InstantiateHair(SpeciesExport creature)
        {
            // TODO: how to know if it has hair
            //if (creature.stats. == -1) return;
            GameObject go = Instantiate(hair, transform);
            go.transform.localScale = Vector3.one * creature.stats.ModifyStatByAge(creature.stats.Size) / 100f;
        }

        private void InstantiateBeard(SpeciesExport creature)
        {
            if (creature.stats.LifeSpan <= 30 * 50 * 24 * 365) return; // TODO: this time configurable with how much is a year
            GameObject go = Instantiate(beard, transform);
            go.transform.localScale = Vector3.one * creature.stats.ModifyStatByAge(creature.stats.Size) / 100f;
        }

        private void InstantiateStatusBar(SpeciesExport creature)
        {
            StatusBar go = Instantiate(statusBar, transform).GetComponent<StatusBar>();
            go.SetName(creature.name);
            go.transform.localScale = Vector3.one * creature.stats.ModifyStatByAge(creature.stats.Size) / 100f;
            go.transform.localPosition = Vector3.up * 3 * creature.stats.ModifyStatByAge(creature.stats.Size) / 100f;
        }
    }
}