using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearResultLoader : MonoBehaviour
{
    public class Desc
    {
        public ClearResult.Desc desc = null;
    }

    Desc desc = null;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= this.OnSceneLoaded;

        ClearResult clearResult = ClearResult.Find();

        clearResult.ApplyDesc(desc.desc);

        Destroy(this.gameObject);
    }

    public static ClearResultLoader Load(Desc desc)
    {
        GameObject loaderObject = new GameObject();
        ClearResultLoader loader = loaderObject.AddComponent<ClearResultLoader>();
        loader.desc = desc;

        DontDestroyOnLoad(loaderObject);
        SceneManager.sceneLoaded += loader.OnSceneLoaded;

        SceneManager.LoadScene(Scenes.Clear, LoadSceneMode.Single);

        return loader;
    }
}
