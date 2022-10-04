using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class SCMA_Loader : MonoBehaviour
{
    public static IEnumerator LoadScene(string SceneName, LoadSceneMode mode, Text text, Image image)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(SceneName, mode);
        text.text = "0%";
        image.fillAmount = 0;
        yield return null;

        async.allowSceneActivation = false;

        float p = 0;
        while (async.progress < 0.9f)
        {
            p = async.progress;
            text.text = p + "%";
            image.fillAmount = p;
            yield return null;
        }
        async.allowSceneActivation = true;

        while (async.progress < 1f)
        {
            p = async.progress;
            text.text = p + "%";
            image.fillAmount = p;
            yield return null;
        }
        yield break;
    }
    public static IEnumerator LoadScene(string SceneName, LoadSceneMode mode)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(SceneName, mode);

        yield return null;

        async.allowSceneActivation = false;

        float p = 0;
        while (async.progress < 0.9f)
        {
            p = async.progress;
            print(p + '%');
            yield return null;
        }
        async.allowSceneActivation = true;

        while (async.progress < 1f)
        {
            p = async.progress;
            print(p + '%');
            yield return null;
        }
        yield break;
    }




}
