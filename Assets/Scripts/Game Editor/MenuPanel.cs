using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class MenuPanel : MonoBehaviour
{
    protected Mediator mediator => Mediator.i;

    [SerializeField]
    Button newButton = null;

    [SerializeField]
    Button loadButton = null;

    [SerializeField]
    Button saveButton = null;

    [SerializeField]
    Button applyButton = null;

    [SerializeField]
    Button notePlacerButton = null;

    [SerializeField]
    Button noteModifierButton = null;

    [SerializeField]
    Button noteInspectorButton = null;

    [SerializeField]
    Button BPMInspectorButton = null;

    [SerializeField]
    Button speedScaleInspectorButton = null;

    [SerializeField]
    Button mapTypeInspectorButton = null;

    [SerializeField]
    Button musicConfigInspectorButton = null;

    [SerializeField]
    Button playSettingsButton = null;

    [SerializeField]
    Button playButton = null;

    [SerializeField]
    Button playAtTimeButton = null;

    [SerializeField]
    Button beatIncreaseButton = null;
    TMP_Text beatIncreaseButtonText = null;

    string lastPath = "";

    private void Awake()  
    {
        newButton.onClick.AddListener(OnNewButttonClick);
        loadButton.onClick.AddListener(OnLoadButtonClick);
        saveButton.onClick.AddListener(OnSaveButtonClick);
        applyButton.onClick.AddListener(OnApplyButtonClick);
        notePlacerButton.onClick.AddListener(OnNotePlacerButtonClick);
        noteModifierButton.onClick.AddListener(OnNoteModifierButtonClick);
        noteInspectorButton.onClick.AddListener(OnNoteInspectorButtonClick);
        BPMInspectorButton.onClick.AddListener(OnBPMInspectorButtonClick);
        speedScaleInspectorButton.onClick.AddListener(OnSpeedScaleInspectorButtonClick);
        mapTypeInspectorButton.onClick.AddListener(OnMapTypeInspectorButtonClick);
        musicConfigInspectorButton.onClick.AddListener(OnMusicConfigInspectorButtonClick);
        playSettingsButton.onClick.AddListener(OnPlaySettingsButtonClick);
        playButton.onClick.AddListener(OnPlayButtonClick);
        playAtTimeButton.onClick.AddListener(OnPlayAtTimeButtonClick);
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
            if (Input.GetKeyDown(Keys.NotePlacer))
            {
                OnNotePlacerButtonClick();
            }
            else if (Input.GetKeyDown(Keys.NoteModifier))
            {
                OnNoteModifierButtonClick();
            }

            if (Input.GetKeyDown(Keys.ViewportBeatIncrease))
            {
                OnBeatIncreaseButtonClick();
            }
        }
    }

    void OnLoadFile(string[] paths)
    {
        mediator.music.LoadSerializedMusic(paths[0]);
        lastPath = paths[0];
    }

    private void OnNewButttonClick()
    {
        mediator.music.ResetMusic();
    }

    private void OnLoadButtonClick()
    {
        mediator.gameEditor.Load();
    }

    private void OnSaveButtonClick()
    {
        mediator.gameEditor.Save();
    }

    private void OnApplyButtonClick()
    {
        mediator.gameEditor.SaveToCurrent();
    }

    private void OnNotePlacerButtonClick()
    {
        mediator.notePlacer.gameObject.SetActive(true);
        mediator.noteModifier.gameObject.SetActive(false);
    }

    private void OnNoteModifierButtonClick()
    {
        mediator.notePlacer.gameObject.SetActive(false);
        mediator.noteModifier.gameObject.SetActive(true);
    }

    private void OnNoteInspectorButtonClick()
    {
        mediator.inspectorPanel.ShowInspector(typeof(NoteInspector));
    }

    private void OnBPMInspectorButtonClick()
    {
        mediator.inspectorPanel.ShowInspector(typeof(BPMInspector));
    }

    private void OnSpeedScaleInspectorButtonClick()
    {
        mediator.inspectorPanel.ShowInspector(typeof(SpeedScaleInspector));
    }

    private void OnMapTypeInspectorButtonClick()
    {
        mediator.inspectorPanel.ShowInspector(typeof(MapTypeInspector));
    }

    private void OnMusicConfigInspectorButtonClick()
    {
        mediator.inspectorPanel.ShowInspector(typeof(MusicConfigInspector));
    }

    private void OnPlaySettingsButtonClick()
    {
        mediator.playSettingsPanel.Show();
    }

    private void UpdateBeatIncreaseButtonText()
    {
        beatIncreaseButtonText.text = mediator.noteViewport.beatType.ToString();
    }

    private void OnPlayButtonClick()
    {
        mediator.gameEditor.Play();
    }

    private void OnPlayAtTimeButtonClick()
    {
        mediator.gameEditor.PlayAt();
    }

    private void OnBeatIncreaseButtonClick()
    {
        mediator.noteViewport.IncreaseBeat();
        UpdateBeatIncreaseButtonText();
    }
}
