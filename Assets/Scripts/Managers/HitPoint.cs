using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitPoint : MonoBehaviour
{
    protected Mediator mediator => Mediator.i;

    [SerializeField]
    GameObject airPoint = null;

    [SerializeField]
    GameObject roadPoint = null;

    [SerializeField]
    GameObject musicLine = null;
    
    public GameObject air
    {
        get => airPoint;
    }

    public GameObject road
    {
        get => roadPoint;
    }

    public Vector2 Pos
    {
        get => transform.position;
    }

    public Vector2 LocalPos
    {
        get => transform.localPosition;
    }

    public Vector2 airPos
    {
        get => airPoint.transform.position;
    }

    public Vector2 airLocalPos
    {
        get => airPoint.transform.localPosition;
    }

    public Vector2 roadPos
    {
        get => roadPoint.transform.position;
    }

    public Vector2 roadLocalPos
    {
        get => roadPoint.transform.localPosition;
    }

    public float airWorldHeight
    {
        get => Mathf.Abs(airPoint.transform.position.y - transform.position.y);
    }

    public float roadWorldHeight
    {
        get => Mathf.Abs(transform.position.y - roadPoint.transform.position.y);
    }

    public void Init()
    {
        bool hitPointActive = !mediator.gameSettings.isEditor;
        bool musicLineActive = mediator.gameSettings.isEditor;

        airPoint.gameObject.SetActive(hitPointActive);
        roadPoint.SetActive(hitPointActive);
        musicLine.SetActive(musicLineActive);
    }

    private void Awake()
    {
        Init();
    }
}
