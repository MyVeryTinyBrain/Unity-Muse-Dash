using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FailResultLoader : MonoBehaviour
{
    public class Desc
    {
        public FailResult.Desc desc = null;
    }

    Desc desc = null;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= this.OnSceneLoaded;

        FailResult failResult = FailResult.Find();

        failResult.ApplyDesc(desc.desc);

        Destroy(this.gameObject);
    }

    public static FailResultLoader Load(Desc desc)
    {
        GameObject loaderObject = new GameObject();
        FailResultLoader loader = loaderObject.AddComponent<FailResultLoader>();
        loader.desc = desc;

        DontDestroyOnLoad(loaderObject);
        SceneManager.sceneLoaded += loader.OnSceneLoaded;

        SceneManager.LoadScene(Scenes.Fail, LoadSceneMode.Single);

        return loader;
    }
}
