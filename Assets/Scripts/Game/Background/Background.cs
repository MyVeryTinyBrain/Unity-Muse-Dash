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
        // 음악 재생시간에 알맞은 배경 스킨을 이진탐색해 맵 스킨을 변경
        MapType currentMap = mediator.music.GetMapTypeAtTime(mediator.music.playingTime);
        if(currentMap != prevMap)
        {
            prevMap = currentMap;
            backgrounds.ForEach((x) => x.SetActive(false));
            backgrounds[(int)currentMap].SetActive(true);
        }

        // 배경 오브젝트를 음악 재생시간에 알맞은 위치로 이동 또는 애니메이션 재생
        foreach(IBackgroundObject background in backgroundObjects)
        {
            background.SetTime(mediator.music.playingTime);
        }
    }
}
