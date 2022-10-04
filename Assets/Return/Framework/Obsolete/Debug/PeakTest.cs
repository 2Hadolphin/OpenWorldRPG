using UnityEngine;

public class PeakTest : MonoBehaviour
{
    public GameObject[] TestObject;
    public Vector2 TestNumber;
    public int Range;
    public Transform Parent;


    public void OnEnable()
    {
        int length= TestObject.Length;
        for(int obj = 0; obj < length; obj++)
        {
            for (int x = 0; x < TestNumber.x; x++)
            {
                for (int z = 0; z < TestNumber.y; z++)
                {
                    Instantiate(TestObject[obj], new Vector3(x * Range,obj*20+5 , z * Range), Quaternion.identity,Parent);
                }
            }
        }

    }

}
