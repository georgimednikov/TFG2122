using UnityEngine;

namespace UnitySimulation
{
    public class DayNightCycle : MonoBehaviour
    {
        public float nightLight = 0.1f;
        public float dayLight = 1;
        public Material skyboxMaterial;

        float lightDiff;
        Light lightSource;

        private void Start()
        {
            lightDiff = dayLight - nightLight;
            lightSource = GetComponent<Light>();
        }

        // Update is called once per frame
        void Update()
        {
            float currentTicks = SimulationManager.Instance.GetCurrentTicks();
            float ticksInDay = SimulationManager.Instance.GetTicksInDay();
            float ticksInHalfDay = SimulationManager.Instance.GetTicksInDay() / 2.0f;

            float currentDayTicks = currentTicks % ticksInDay;
            float blend = currentDayTicks / ticksInHalfDay;
            if (currentDayTicks > ticksInHalfDay)
            {
                //*Debug.Log("SE METE " + blend);
                blend = 1 - (blend - 1);
            }

            //Debug.Log("CurrentTicks: " + currentTicks + " TicksInHalfDay: " + ticksInHalfDay + " Blend: " + blend);

            skyboxMaterial.SetFloat("_Blend", blend);
            lightSource.intensity = Lerp(blend);
        }

        float Lerp(float blend)
        {
            return dayLight - (lightDiff * blend);
        }
    }
}
