using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBackgroundObject
{
    public void SetTime(float time);
}

public class Background : MonoBehaviour
{
    protected Mediator mediator => Mediator.i;

    [SerializeField]
    List<GameObject> backgrounds;

    [SerializeField]
    FeverBackground feverBackground = null;

    [SerializeField]
    DeadBackground deadBackground = null;

    List<IBackgroundObject> backgroundObjects = new List<IBackgroundObject>();

    MapType prevMap = MapType._01;

    public FeverBackground fever => feverBackground;
    public DeadBackground dead => deadBackground;

    public void RegistBackgroundObject(IBackgroundObject backgroundObject)
    {
        if (!backgroundObjects.Contains(backgroundObject))
        {
            backgroundObject.SetTime(mediator.music.playingTime);
            backgroundObjects.Add(backgroundObject); 
        }
    }

    public void UnregistBackgroundObject(IBackgroundObject backgroundObject)
    {
        backgroundObjects.Remove(backgroundObject);
    }

    private void Awake()
    {
        IBackgroundObject[] finded = GetComponentsInChildren<IBackgroundObject>();
        backgroundObjects.AddRange(finded);

        if(backgrounds.Count > 0)
        {
            backgrounds.ForEach((x) => x.SetActive(false));
            backgrounds[0].SetActive(true);
        }
    }

    private void Update()
    {
        MapType currentMap = mediator.music.GetMapTypeAtTime(mediator.music.playingTime);
        if(currentMap != prevMap)
        {
            prevMap = currentMap;
            backgrounds.ForEach((x) => x.SetActive(false));
            backgrounds[(int)currentMap].SetActive(true);
        }

        foreach(IBackgroundObject background in backgroundObjects)
        {
            background.SetTime(mediator.music.playingTime);
        }
    }
}
