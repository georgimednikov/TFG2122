using UnityEngine;

namespace EvolutionSimulation.Unity
{
    public class InterpretJSON : MonoBehaviour
    {
        public GameObject body;
        public GameObject leg;
        public GameObject[] mouths;
        public GameObject armour;

        // Start is called before the first frame update
        public void CreateCreature(TextAsset json)
        {
            SpeciesExport creature = JsonUtility.FromJson<SpeciesExport>(json.text);

            InstantiateBody(creature);

            InstantiateLegs(creature);

            InstantiateMouth(creature);

            InstantiateArmour(creature);
        }

        private void InstantiateMouth(SpeciesExport creature)
        {
            GameObject go = Instantiate(mouths[(int)creature.stats.Diet], transform);
            go.transform.localScale = Vector3.one * creature.stats.ModifyStatByAge(creature.stats.Size) / 100f;
            go.transform.localPosition = Vector3.up * creature.stats.ModifyStatByAge(creature.stats.Size) / 100f * 1.5f;
        }

        private void InstantiateArmour(SpeciesExport creature)
        {
            GameObject go = Instantiate(armour, transform);
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
    }
}