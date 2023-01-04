using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PauseMenu : MonoBehaviour, IGameReset
{
    protected Mediator mediator => Mediator.i;
    
    class ButtonAction
    {
        PauseMenuButton button = null;
        Action action = null;

        public PauseMenuButton btn => button;

        public ButtonAction(PauseMenuButton button, Action action)
        {
            this.button = button;
            this.action = action;
        }

        public void SetSelect(bool value)
        {
            button.SetSelect(value);
        }

        public void Execute()
        {
            action?.Invoke();
        }
    }

    [SerializeField]
    GameObject body = null;

    [SerializeField]
    new Animation animation = null;

    [SerializeField]
    AnimationClip showClip = null;

    [SerializeField]
    AnimationClip hideClip = null;

    [SerializeField]
    PauseMenuButton backButton = null;

    [SerializeField]
    PauseMenuButton restartButton = null;

    [SerializeField]
    PauseMenuButton resumeButton = null;

    string currentAnimation = "";
    float animationTime = 0.0f;
    bool show = false;

    int select = 0;
    List<ButtonAction> buttonActions = new List<ButtonAction>();

    public void HideImmediate()
    {
        show = false;
        body.SetActive(false);
    }

    void PlayAnimation(AnimationClip clip)
    {
        animation.Stop();
        animation.Play(clip.name);
        animation[clip.name].speed = 0.0f;

        currentAnimation = clip.name;
        animationTime = 0.0f;
    }

    void UpdateAnimation()
    {
        if (animation.isPlaying)
        {
            animationTime += Time.unscaledDeltaTime;
            animation[currentAnimation].time = animationTime;
        }
    }

    bool IsEmpty()
    {
        return buttonActions.Count == 0;
    }

    void PauseAndShow()
    {
        show = true;

        PlayAnimation(showClip);

        mediator.music.Pause();
        Time.timeScale = 0.0f;
    }

    void ResumeAndHide()
    {
        show = false;

        PlayAnimation(hideClip);

        mediator.music.Resume();
        Time.timeScale = 1.0f;
    }

    void ResetButtonSelectImage()
    {
        buttonActions.ForEach((x) => x.SetSelect(false));
    }

    void Select(PauseMenuButton button)
    {
        for (int i = 0; i < buttonActions.Count; ++i)
        {
            if (buttonActions[i].btn == button)
            {
                Select(i);
                break;
            }
        }
    }

    void Select(int index)
    {
        if (IsEmpty())
        {
            return;
        }

        ResetButtonSelectImage();
        if (index < 0)
        {
            index = buttonActions.Count - 1;
        }
        select = index % buttonActions.Count;
        buttonActions[select].SetSelect(true);
    }

    void ExecuteCurrentSelectedButton()
    {
        if (IsEmpty())
        {
            return;
        }

        buttonActions[select].Execute();
    }

    void OnClickButton(PauseMenuButton button)
    {
        if (button == buttonActions[select].btn)
        {
            ExecuteCurrentSelectedButton();
        }
        else
        {
            Select(button);
        }
    }

    void Back()
    {
        AudioListener.pause = false;
        Time.timeScale = 1.0f;
        Loader.LoadInEditor(Loader.GetRecordedState());
    }

    void Restart()
    {
        AudioListener.pause = false;
        Time.timeScale = 1.0f;
        Loader.LoadInEditor(Loader.GetLastLoadDesc());
    }

    private void Awake()
    {
        HideImmediate();

        backButton.onClick += OnClickButton;
        restartButton.onClick += OnClickButton;
        resumeButton.onClick += OnClickButton;

        buttonActions.Add(new ButtonAction(backButton, () =>
        {
            Back();
            HideImmediate();
        }));
        buttonActions.Add(new ButtonAction(restartButton, () =>
        {
            Restart();
            ResumeAndHide();
        }));
        buttonActions.Add(new ButtonAction(resumeButton, () =>
        {
            ResumeAndHide();
        }));

        Select(0);
    }

    void UpdateWhileShow()
    {
        if (Input.GetKeyDown(Keys.PauseMenuLeft))
        {
            Select(select - 1);
        }
        if (Input.GetKeyDown(Keys.PauseMenuRight))
        {
            Select(select + 1);
        }
        if (Input.GetKeyDown(Keys.PauseMenuSelect))
        {
            ExecuteCurrentSelectedButton();
        }
    }

    private void Update()
    {
        if (mediator.music.isPaused && !show)
        {
            PauseAndShow();
        }

        if (Input.GetKeyDown(Keys.PauseGame))
        {
            if (!mediator.music.isPaused)
            {
                PauseAndShow();
            }
            else
            {
                ResumeAndHide();
            }
        }

        if (show)
        {
            UpdateWhileShow();
        }

        UpdateAnimation();
    }

    public void GameReset()
    {
        HideImmediate();
    }
}
