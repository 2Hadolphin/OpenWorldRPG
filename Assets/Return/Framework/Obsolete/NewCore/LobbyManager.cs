using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{ 
    public virtual void RunGame()
    {
        var progress = new GameObject();//Instantiate(AssetsCatch.Go.ProgressBar);
        var _fill = progress.GetComponentInChildren<Image>();
        var _count = progress.GetComponentInChildren<Text>();
        
        StartCoroutine(SCMA_Loader.LoadScene(
            "ReturnMode",
            UnityEngine.SceneManagement.LoadSceneMode.Single,
              _count,
              _fill));
    }
}
