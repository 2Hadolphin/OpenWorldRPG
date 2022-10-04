using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AIPathTargetDisplay : MonoBehaviour
{
    [SerializeField]
    private Image targetUI=null;
    [SerializeField]
    private RectTransform TF=null;


    private void Start()
    {

    }

    public void setTarget(Transform tf)
    {
        StopAllCoroutines();
        TF.position = tf.position;

        targetUI.enabled = true;
        StartCoroutine(CloseTarget());
    }
    public void setTarget(Vector3 v3)
    {
        StopAllCoroutines();
        TF.position = v3;
        targetUI.enabled = true;

        StartCoroutine(CloseTarget());

    }

    IEnumerator CloseTarget()
    {
        
        yield return new WaitForSeconds(2.5f);
        targetUI.enabled = false;
    }


}
