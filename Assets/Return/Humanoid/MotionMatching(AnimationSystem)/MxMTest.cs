using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MxM;
using Sirenix.OdinInspector;
using Return;

public class MxMTest : MonoBehaviour
{
    MxMAnimator animator;

    private void Start()
    {
        animator = gameObject.GetComponent<MxMAnimator>();
        var input = InputManager.Input;
        input.Battle.Switch.performed += Switch_performed;

    }

    private void Switch_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Tag_Firearms();
    }

    [Button("Stance")]
    void Tag_Stance()
    {

    }

    [Button("Firearms")]
    void Tag_Firearms()
    {
        if (!firearms)
            animator.AddRequiredTag("Firearms");
        else
            animator.RemoveRequiredTag("Firearms");
        firearms = !firearms;
    }

    bool firearms;
}
