using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadBackground : MonoBehaviour
{
    public delegate void OnEndPlayDelegate();
    public event OnEndPlayDelegate OnEndPlay;

    [SerializeField]
    new Animation animation = null;

    [SerializeField]
    SpriteRenderer fillSprite = null;

    bool playing = false;
    Coroutine coroutine_Show = null;

    public void ShowAndPlay(float delay = 0.0f)
    {
        if (coroutine_Show != null)
        {
            StopCoroutine(coroutine_Show);
        }

        coroutine_Show = StartCoroutine(Coroutine_Show(delay));
    }

    IEnumerator Coroutine_Show(float delay)
    {
        if (delay > 0.0f)
        {
            yield return new WaitForSeconds(delay);
        }

        animation.Stop();
        animation.Play();
        playing = true;

        coroutine_Show = null;
        yield return null;
    }

    public void Hide()
    {
        animation.Stop();
        playing = false;

        fillSprite.gameObject.SetActive(false);
    }

    private void Awake()
    {
        animation.playAutomatically = false;
        fillSprite.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (playing && !animation.isPlaying)
        {
            OnEndPlay?.Invoke();
            playing = false;
        }
    }
}
