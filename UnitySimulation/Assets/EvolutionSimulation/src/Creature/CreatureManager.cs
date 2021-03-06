using EvolutionSimulation.Entities;
using EvolutionSimulation.Genetics;
using UnityEngine;

namespace UnitySimulation
{
    /// <summary>
    /// Manages the object to represent the simulation creatures.
    /// Each part is generated according to the data of the simulation 
    /// or with the species exported data.
    /// </summary>
    public class CreatureManager : MonoBehaviour
    {
        public GameObject body;
        public GameObject[] legs;

        public GameObject[] mouths;
        public Material mouthMat;

        public GameObject[] armours;

        public GameObject spikes;

        public GameObject hair;
        public Material hairMat;

        public GameObject beard;
        public Material beardMat;

        public GameObject[] wings;
        public GameObject statusBar;

        private StatusBar _statusBar;

        GameObject bodyMid, bodyForward, bodyBack;
        Transform bodyEmpty;
        AnimationActivation[] legAnimations; 
        AnimationActivation wingAnimation;

        float sizeScale;
        float baseHeight;

        /// <summary>
        /// Creates the creature using the simulation creature data.
        /// </summary>
        public void InitalizeCreature(Creature creature)
        {
            SpeciesExport species = new SpeciesExport(creature.speciesName, creature.stats);
            Init(species);
        }

        /// <summary>
        /// Creates the creature using the species json.
        /// </summary>
        public void InitalizeCreature(TextAsset json)
        {
            SpeciesExport species = SpeciesExport.GetExportFromJSON(json.text);
            Init(species);
        }

        void Init(SpeciesExport species)
        {
            sizeScale = species.stats.Size / 100f;
            baseHeight = sizeScale / 1.25f;
            bodyEmpty = new GameObject("BodyEmpty").transform;
            bodyEmpty.parent = transform;
            bodyEmpty.localPosition = Vector3.zero;
            bodyEmpty.Translate(Vector3.up * (sizeScale + baseHeight));

            InstantiateBody(species);

            InstantiateMouth(species);

            // InstantiateArmour(creature);

            InstantiateSpikes(species);

            InstantiateHair(species);

            InstantiateBeard(species);

            InstantiateWings(species);

            if (species.stats.Upright)
            {
                bodyEmpty.transform.Rotate(Vector3.right * -90);
            }

            InstantiateStatusBar(species);

            InstantiateLegs(species);
        }

        #region Creature Meshes

        private void InstantiateMouth(SpeciesExport creature)
        {
            GameObject go = Instantiate(mouths[(int)creature.stats.Diet], bodyEmpty);
            go.transform.Translate(-Vector3.up * (bodyForward.transform.localPosition.z + bodyForward.transform.localScale.z / 2));
            go.transform.Rotate(Vector3.forward * 180);
            go.transform.localScale = Vector3.one * bodyForward.transform.localScale.z;
            go.GetComponent<MeshRenderer>().material = mouthMat; 
        }

        private void InstantiateArmour(SpeciesExport creature)
        {
            GameObject go;
            if (creature.stats.Armor >= 15)
                go = Instantiate(armours[1], bodyEmpty);
            else if (creature.stats.Armor >= 8)
                go = Instantiate(armours[0], bodyEmpty);
            else return; // If Armor is low, don't put on armor
            go.transform.localScale = Vector3.one * sizeScale;
        }

        private void InstantiateBody(SpeciesExport creature)
        {
            bodyMid = Instantiate(body, bodyEmpty);
            bodyMid.transform.localScale = Vector3.one * sizeScale * ((creature.stats.MaxHealth / 55f * 0.75f) + 0.5f); // 55: median value of health
            bodyMid.GetComponent<MeshRenderer>().material.color = new Color(1, 1 - creature.stats.MaxHealth / 110f, 1 - creature.stats.MaxHealth / 110f);

            bodyForward = Instantiate(body, bodyEmpty);
            bodyForward.transform.localScale = Vector3.one * sizeScale * ((creature.stats.Aggressiveness / 10f * 0.75f) + 0.5f);
            bodyForward.transform.Translate(Vector3.forward * ((bodyMid.transform.localScale.z + bodyForward.transform.localScale.z) / 2));
            bodyForward.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1 - creature.stats.Aggressiveness / 10f);

            bodyBack = Instantiate(body, bodyEmpty);
            bodyBack.transform.localScale = Vector3.one * sizeScale * ((creature.stats.Armor / 12.5f * 0.75f) + 0.5f);
            bodyBack.transform.Translate(-Vector3.forward * ((bodyMid.transform.localScale.z + bodyBack.transform.localScale.z) / 2));
            bodyBack.GetComponent<MeshRenderer>().material.color = new Color(1 - creature.stats.Armor / 25f, 1, 1 - creature.stats.Armor / 25f);
        }

        private void InstantiateLegs(SpeciesExport creature)
        {
            int legIndex = (int)(creature.stats.GroundSpeed / 200.0f * legs.Length);
            GameObject go;
            GameObject legParent = new GameObject("Leg Cluster");
            legParent.transform.parent = transform;
            legParent.transform.localPosition = Vector3.up * (sizeScale + baseHeight);
            legParent.transform.localScale = Vector3.one * sizeScale;
            float deltaAngle = 2 * Mathf.PI / creature.stats.Limbs;
            legAnimations = new AnimationActivation[creature.stats.Limbs];
            for (int i = 0; i < creature.stats.Limbs; i++)
            {
                go = Instantiate(legs[legIndex], legParent.transform);
                go.transform.rotation = Quaternion.Euler(-90, -i * deltaAngle / Mathf.PI * 180, -90);
                legAnimations[i] = go.GetComponent<AnimationActivation>();
            }
            if (creature.stats.Limbs % 2 == 1) legParent.transform.Rotate(new Vector3(0, -90, 0));
        }

        private void InstantiateSpikes(SpeciesExport creature)
        {
            if (creature.stats.Counter == -1) return; // TODO: how to check if the creture has this ability?
            GameObject go = Instantiate(spikes, bodyEmpty);
            go.transform.localScale = Vector3.one * sizeScale;
            go.transform.Translate(Vector3.up * (bodyMid.transform.localScale.y / 4));
        }

        private void InstantiateHair(SpeciesExport creature)
        {
            if (!creature.stats.Hair) return;
            GameObject go = Instantiate(hair, bodyEmpty);
            go.transform.localScale = Vector3.one * sizeScale;
            go.transform.localPosition = bodyForward.transform.localPosition + Vector3.forward * 0.00528117f; // To make his toupee excel
            go.transform.Translate(Vector3.forward * (bodyForward.transform.localScale.y / 2 + go.transform.localScale.y / 20));
        }

        private void InstantiateBeard(SpeciesExport creature)
        {
            if (creature.stats.LifeSpan <= 30 * 50 * 24 * 365) return; // TODO: this time configurable with how much is a year
            GameObject go = Instantiate(beard, bodyEmpty);
            go.transform.localScale = Vector3.one * sizeScale;
            go.transform.localPosition = bodyForward.transform.localPosition + Vector3.down * (bodyForward.transform.localScale.z / 2 + go.transform.localScale.z / 4) + Vector3.forward * bodyForward.transform.localScale.z / 3;
        }

        private void InstantiateWings(SpeciesExport creature)
        {
            if (creature.stats.AerialSpeed <= 0) return;
            int i = creature.stats.AerialSpeed / 100;
            GameObject go = Instantiate(wings[i], bodyEmpty);
            go.transform.localScale = Vector3.one * sizeScale;
            go.transform.Translate(go.transform.up * (sizeScale + baseHeight) / 3);
            wingAnimation = go.GetComponent<AnimationActivation>();
            //go = Instantiate(wings[i], bodyEmpty);
            //go.transform.localScale = Vector3.one * sizeScale;
            //go.transform.Translate(-go.transform.right * (sizeScale + baseHeight) / 3);

        }

        private void InstantiateStatusBar(SpeciesExport creature)
        {
            _statusBar = Instantiate(statusBar, transform).GetComponent<StatusBar>();
            //_statusBar.transform.position = new Vector3(_statusBar.transform.position.x, _statusBar.transform.position.y, _statusBar.transform.position.z);
            _statusBar.SetName(creature.name);
            _statusBar.transform.localScale = Vector3.one * sizeScale;
            _statusBar.transform.Translate(Vector3.up * 4 * (sizeScale + baseHeight));
        }
      
        public void SetStatusBar(float percentage)
        {
            _statusBar.SetHealthBarPercentage(percentage);
        }
        public void SetStatusTexts(string state, string info)
        {
            _statusBar.SetStatus(state);
            _statusBar.SetStatusInfo(info);
        }

        public void ActivateLegAnimation(bool activation)
        {
            foreach (AnimationActivation animAct in legAnimations)
                animAct.Activation(activation);
        }
        public void ActivateWingsAnimation(bool activation)
        {
            if(wingAnimation != null)
                wingAnimation.Activation(activation);
        }

        #endregion
    }
}

