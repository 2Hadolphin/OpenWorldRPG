using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TNet;

using Return;

public class TNetTest : MonoBehaviour
{

    private void OnDestroy()
    {
        if(TryGetComponent<TNObject>(out var tno))
        {
            tno.DestroySelf();
        }
    }

    IEnumerator Start()
    {
        TNObject tno;

        while (!TryGetComponent(out tno))
        {
            Debug.Log(Time.frameCount);
            yield return null;
        }

        if (tno.isMine)
        {
            var input = InputManager.Input;
            input.Humanoid.Jump.performed += Jump_performed;
            input.Enable();
        }
        else
            Debug.LogError(tno);
    }

    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        RandomColor();
    }

    void RandomColor()
    {

        if (TryGetComponent<TNObject>(out var tno))
        {
            var color = Random.ColorHSV();
            tno.Send(nameof(SetColor), Target.All, color);
            Debug.Log("Send : "+color);
        }

    }

    [RFC]
    public void SetColor(Color color)
    {
        Debug.Log("Recive : "+color);
        var ren=GetComponent<Renderer>();
        ren.material.color = color;
    }
}
