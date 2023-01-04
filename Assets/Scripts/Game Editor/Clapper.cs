using UnityEngine;

public class Clapper : MonoBehaviour
{
    [SerializeField]
    AudioSource audioSoruce = null;

    int lastBeatIndex = -1;

    protected Mediator mediator => Mediator.i;

    private void OnEnable()
    {
        lastBeatIndex = mediator.music.GetClosetBeatIndexAtTime(mediator.music.playingTime, BeatType.B4);
    }

    private void Update()
    {
        // Beat output
        lastBeatIndex = Mathf.Clamp(lastBeatIndex, -1, int.MaxValue);
        int NextBeatIndex = lastBeatIndex + 1;
        if (NextBeatIndex < mediator.music.beats4.Count)
        {
            if (lastBeatIndex >= 0 && mediator.music.playingTime <= mediator.music.beats4[lastBeatIndex])
            {
                int closetIndex = mediator.music.GetClosetBeatIndexAtTime(mediator.music.playingTime, BeatType.B4);
                lastBeatIndex = closetIndex - 1;
            }
            else if (mediator.music.playingTime >= mediator.music.beats4[NextBeatIndex])
            {
                if (mediator.music.isPlaying)
                {
                    audioSoruce.Play();
                }

                lastBeatIndex = NextBeatIndex;
            }
        }
    }
}
