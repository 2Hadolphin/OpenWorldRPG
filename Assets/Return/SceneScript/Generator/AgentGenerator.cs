using UnityEngine;

namespace Return.SceneModule
{
    public class AgentGenerator : AssetsGenerator
    {
        public override void Generate()
        {
            generateEnemy(15);
        }



        public override void Initialization()
        {

        }

        private void generateEnemy(int amont)
        {
            for (int i = 0; i < amont; i++)
            {
                Vector3 position = new Vector3(Random.Range(-50f, 50f), Random.Range(10f, 20f), Random.Range(-50f, -50));
                Instantiate(pool.Assets.Agent[0], position, Quaternion.identity);
            }

        }
    }
}