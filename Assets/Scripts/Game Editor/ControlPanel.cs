using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ControlPanel : MonoBehaviour
{
    protected Mediator mediator => Mediator.i;

    [SerializeField]
    Button playButton = null;

    [SerializeField]
    Button stopButton = null;

    [SerializeField]
    Button resumeOrPauseButton = null;

    [SerializeField]
    Slider volumeSlider = null;

    [SerializeField]
    Slider pitchSlider = null;

    [SerializeField]
    Slider timeSlider = null; 

    [SerializeField]
    TMP_InputField minuteFiled = null;

    [SerializeField]
    TMP_InputField secondFiled = null;

    [SerializeField]
    TMP_InputField milliSecondFiled = null;

    [SerializeField]
    TMP_InputField readOnlyTimeFiled = null;

    [SerializeField]
    Button moveToLeftBeatButton = null;

    [SerializeField]
    Button moveToRightBeatButton = null;

    [SerializeField]
    Button fitToClosetBeatButton = null;

    [SerializeField]
    Slider globalSpeedScaleMultiplierSlider = null;

    [SerializeField]
    Button beatIncreaseButton = null;
    TMP_Text beatIncreaseButtonText = null;

    private void Awake()
    {
        playButton.onClick.AddListener(OnPlayButtonClick);
        stopButton.onClick.AddListener(OnStopButtonClick);
        resumeOrPauseButton.onClick.AddListener(OnResumeOrPauseButtonClick);
        timeSlider.onValueChanged.AddListener(OnTimeSliderChanged);
        minuteFiled.onValueChanged.AddListener(OnMinuteFiledChanged);
        secondFiled.onValueChanged.AddListener(OnSecondFiledChanged);
        milliSecondFiled.onValueChanged.AddListener(OnMilliSecondFiledChanged);
        volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);
        pitchSlider.onValueChanged.AddListener(OnPitchSliderChanged);
        moveToLeftBeatButton.onClick.AddListener(OnMoveToLeftBeatButtonClick);
        moveToRightBeatButton.onClick.AddListener(OnMoveToRightBeatButtonClick);
        fitToClosetBeatButton.onClick.AddListener(OnFitToClosetBeatButtonClick);
        globalSpeedScaleMultiplierSlider.onValueChanged.AddListener(OnGlobalSpeedScaleMuliplierSliderChanged);
        EventTrigger eventTrigger = globalSpeedScaleMultiplierSlider.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry globalSpeedScaleSliderPointerDown = new EventTrigger.Entry();
        globalSpeedScaleSliderPointerDown.eventID = EventTriggerType.PointerDown;
        globalSpeedScaleSliderPointerDown.callback.AddListener((data) => OnGlobalSpeedScaleMuliplierSliderPointerDown());
        EventTrigger.Entry globalSpeedScaleSliderPointerUp = new EventTrigger.Entry();
        globalSpeedScaleSliderPointerUp.eventID = EventTriggerType.PointerUp;
        globalSpeedScaleSliderPointerUp.callback.AddListener((data) => OnGlobalSpeedScaleMuliplierSliderPointerUp());
        eventTrigger.triggers.Add(globalSpeedScaleSliderPointerDown);
        eventTrigger.triggers.Add(globalSpeedScaleSliderPointerUp);
        beatIncreaseButton.onClick.AddListener(OnBeatIncreaseButtonClick);
        beatIncreaseButtonText = beatIncreaseButton.GetComponentInChildren<TMP_Text>(true);
    }

    private void Start()
    {
        UpdateBeatIncreaseButtonText();
    }

    private void Update()
    {
        if (!Utility.UsingInputField())
        {
            if (Input.GetKeyDown(Keys.Play))
            {
                OnResumeOrPauseButtonClick();
            }

            if (Input.GetKey(Keys.LCnt) || Input.GetKey(Keys.RCnt))
            {
                if (Input.GetKeyDown(Keys.MoveToLeftBeat))
                {
                    OnMoveToLeftBeatButtonClick();
                }
                if (Input.GetKeyDown(Keys.MoveToRightBeat))
                {
                    OnMoveToRightBeatButtonClick();
                }
            }

            if (Input.GetKeyDown(Keys.VolumeInspectorBeatIncrease))
            {
                OnBeatIncreaseButtonClick();
            }
        }

        timeSlider.SetValueWithoutNotify(mediator.music.normalizedPlayingTime);

        int minute, second, milliSecond;
        MusicUtility.TimeToWatchTime(mediator.music.playingTime, out minute, out second, out milliSecond);
        minuteFiled.SetTextWithoutNotify(minute.ToString());
        secondFiled.SetTextWithoutNotify(second.ToString());
        milliSecondFiled.SetTextWithoutNotify(milliSecond.ToString());
        MusicUtility.TimeToWatchTime(mediator.music.length, out minute, out second, out milliSecond);
        readOnlyTimeFiled.SetTextWithoutNotify($"{minute}:{second}:{milliSecond}");

        volumeSlider.SetValueWithoutNotify(mediator.music.volume);

        pitchSlider.SetValueWithoutNotify(mediator.music.pitch);

        globalSpeedScaleMultiplierSlider.SetValueWithoutNotify(mediator.music.globalSpeedScaleMultiplier);
    }

    private void OnPlayButtonClick()
    {
        mediator.music.Play();
    }

    private void OnStopButtonClick()
    {
        mediator.music.Stop();
    }

    private void OnResumeOrPauseButtonClick()
    {
        mediator.music.TogglePause();
    }

    private void OnTimeSliderChanged(float value)
    {
        mediator.music.normalizedPlayingTime = value;
    }

    private bool TryMergeWatchTime(out float time) 
    {
        int minute, second, milliSecond;
        if (int.TryParse(minuteFiled.text, out minute) &&
            int.TryParse(secondFiled.text, out second) &&
            int.TryParse(milliSecondFiled.text, out milliSecond))
        {
            time = MusicUtility.WatchTimeToTime(minute, second, milliSecond);
            return true;
        }

        time = 0.0f;
        return false;
    }

    private void OnMinuteFiledChanged(string value)
    {
        float time;
        if (TryMergeWatchTime(out time))
        {
            mediator.music.playingTime = time;          
        }
    }

    private void OnSecondFiledChanged(string value)
    {
        float time;
        if (TryMergeWatchTime(out time))
        {
            mediator.music.playingTime = time;
        }
    }

    private void OnMilliSecondFiledChanged(string value)
    {
        float time;
        if (TryMergeWatchTime(out time))
        {
            mediator.music.playingTime = time;
        }
    }

    private void OnVolumeSliderChanged(float value)
    {
        mediator.music.volume = value;
    }

    private void OnPitchSliderChanged(float value)
    {
        mediator.music.pitch = value;
    }

    private void OnMoveToLeftBeatButtonClick()
    {
        List<float> beats = mediator.music.GetBeats(mediator.noteViewport.beatType);
        int index = mediator.music.GetBeatIndexToShift(mediator.music.playingTime, mediator.noteViewport.beatType, HDirection.Left);
        mediator.music.playingTime = beats[index];
    }

    private void OnMoveToRightBeatButtonClick()
    {
        List<float> beats = mediator.music.GetBeats(mediator.noteViewport.beatType);
        int index = mediator.music.GetBeatIndexToShift(mediator.music.playingTime, mediator.noteViewport.beatType, HDirection.Right);
        mediator.music.playingTime = beats[index];
    }

    private void OnFitToClosetBeatButtonClick()
    {
        List<float> beats = mediator.music.GetBeats(mediator.noteViewport.beatType);
        int index = mediator.music.GetNearBeatIndex(mediator.music.playingTime, mediator.noteViewport.beatType);
        mediator.music.playingTime = beats[index];
    }

    private void OnGlobalSpeedScaleMuliplierSliderChanged(float value)
    {
        mediator.music.globalSpeedScaleMultiplier = value;
    }

    float sliderPointerDownValue = 0;
    float sliderPointerDownTime = 0;
    private void OnGlobalSpeedScaleMuliplierSliderPointerDown()
    {
        sliderPointerDownValue = globalSpeedScaleMultiplierSlider.value;
        sliderPointerDownTime = Time.time;
    }

    private void OnGlobalSpeedScaleMuliplierSliderPointerUp()
    {
        if (Mathf.Abs(sliderPointerDownTime - Time.time) > 0.5f &&
            Mathf.Abs(sliderPointerDownValue - globalSpeedScaleMultiplierSlider.value) < 0.01f)
        {
            mediator.music.globalSpeedScaleMultiplier = 1.0f;
        }
    }

    private void UpdateBeatIncreaseButtonText()
    {
        beatIncreaseButtonText.text = mediator.volumeInspector.beatType.ToString();
    }

    private void OnBeatIncreaseButtonClick()
    {
        mediator.volumeInspector.IncreaseBeat();
        UpdateBeatIncreaseButtonText();
    }
}
