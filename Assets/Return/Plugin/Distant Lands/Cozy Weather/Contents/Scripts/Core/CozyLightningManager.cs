// Distant Lands 2021.



using UnityEngine;


namespace DistantLands.Cozy
{
    public class CozyLightningManager : CozyModule
    {


        [HideInInspector]
        public Vector2 lightningTime;
        public GameObject lightningPrefab;
        private float lightningTimer;
        private Transform parent;


        // Start is called before the first frame update
        void Start()
        {
            weatherSphere = CozyWeather.instance;
            if (weatherSphere.weatherProfile.useThunder)
                lightningTime = weatherSphere.weatherProfile.thunderTime;
            lightningTimer = Random.Range(lightningTime.x, lightningTime.y);

            parent = new GameObject().transform;
            parent.parent = transform;
            parent.localPosition = Vector3.zero;
            parent.localEulerAngles = Vector3.zero;
            parent.localScale = Vector3.one;
            parent.name = "Lightning";

        }

        // Update is called once per frame
        void Update()
        {
            if (weatherSphere.weatherProfile.useThunder)
            {
                lightningTime = weatherSphere.weatherProfile.thunderTime;
                lightningTimer -= Time.deltaTime;

                if (lightningTimer <= 0)
                {
                    Strike();
                    lightningTimer = Random.Range(lightningTime.x, lightningTime.y);
                }
            }
        }

        public void Strike()
        {

            Transform i = Instantiate(lightningPrefab, parent).transform;

            i.eulerAngles = Vector3.up * Random.value * 360;



        }
    }
}