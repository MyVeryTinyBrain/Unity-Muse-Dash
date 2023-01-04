using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour, IGameReset
{
    protected Mediator mediator => Mediator.i;

    short comboSortingOrder = 0;
    int currentCombo = 0;
    int nMaxCombo = 0;
    float sumAccuracy = 0;
    int nPerfect = 0;
    int nGreat = 0;
    int nPass = 0;
    int nMiss = 0;
    float score = 0;

    float MaxFever = 100.0f;
    float currentFever = 0.0f;
    float feverDuration = 5.0f;
    bool fever = false;

    Coroutine coroutine_MusicStop = null;

    public int combo => currentCombo;

    public bool isFever => fever;

    public bool allowFullCombo = false;

    public void ClearState()
    {
        currentCombo = 0;
        currentFever = 0;
        sumAccuracy = 0;
        nPerfect = 0;
        nGreat = 0;
        nPass = 0;
        nMiss = 0;
        score = 0;
        mediator.comboCountUI.Clear();
        StopFeverImmediate();
    }

    public void Combo(Note note, NoteResult result)
    {
        if (!result.noCombo)
        {
            currentCombo++;
            nMaxCombo = Mathf.Max(nMaxCombo, currentCombo);

            switch (result.precision)
            {
                case ComboPrecision.Great:
                ++nGreat;
                sumAccuracy += 0.5f;
                break;

                case ComboPrecision.Perfect:
                ++nPerfect;
                sumAccuracy += 1.0f;
                break;
            }

            ApplyComboState();
            SpawnComboResult(result.hitPosition, result.precision);
            AddFever_UsingResult(result);
        }

        AddScore_UsingResult(note, result);

        if(allowFullCombo && !result.noCombo && currentCombo == mediator.music.CalculateMaxCombo())
        {
            mediator.gameUI.Show_FullCombo();
        }
    }

    public void AddScore(float score)
    {
        float multiplier = fever ? 2 : 1;
        this.score += score * multiplier;

        mediator.gameUI.SetScore((int)this.score);
    }

    public void AddAccuracy(float accuracy)
    {
        sumAccuracy += accuracy;
    }

    public void Miss(Note note)
    {
        comboSortingOrder = 0;
        currentCombo = 0;
        nMiss += note.GetMissCount();

        ComboCountUI ui = mediator.comboCountUI;
        ui.state = ComboCountUI.State.Hide;
    }

    public void Pass(NotePosition position, float score)
    {
        SpawnPassResult(position);
        AddScore(score);

        ++nPass;
        sumAccuracy += 1.0f;
    }

    private void AddFever_UsingResult(NoteResult result)
    {
        if (!result.noFever)
        {
            IncreaseFever(result.precision);
        }
    }

    private void AddScore_UsingResult(Note note, NoteResult result)
    {
        float increase = 0;
        switch (result.score)
        {
            case ComboScore.Default:
            increase = note.defaultScore;
            break;

            case ComboScore.Sub:
            increase = note.subScore;
            break;
        }

        float multiplier = 1.0f;
        switch (result.precision)
        {
            case ComboPrecision.Great:
            multiplier *= 0.5f;
            break;

            case ComboPrecision.Perfect:
            multiplier *= 1.0f;
            break;
        }

        int nComboMultipler = Mathf.Clamp(currentCombo, 0, 50);
        float comboMultiplier = nComboMultipler * 0.01f + 1.0f;

        AddScore(increase * multiplier * comboMultiplier);
    }

    private void ApplyComboState()
    {
        ComboCountUI ui = mediator.comboCountUI;
        ComboCountUI.State state = ui.state;

        if (state == ComboCountUI.State.Hide && currentCombo >= 0 && currentCombo < 20)
        {
            ui.state = ComboCountUI.State.Level0;
        }
        else if (state != ComboCountUI.State.Level1 && currentCombo >= 20 && currentCombo < 50)
        {
            ui.state = ComboCountUI.State.Level1;
        }
        else if (state != ComboCountUI.State.Level2 && currentCombo >= 50)
        {
            ui.state = ComboCountUI.State.Level2;
        }

        ui.combo = currentCombo;
    }

    private void SpawnComboResult(NotePosition notePosition, ComboPrecision type)
    {
        Effect effect = mediator.comboResultUI.SpawnResult(notePosition, type);
        effect.sortingOrder = comboSortingOrder++;
    }

    private void SpawnPassResult(NotePosition notePosition)
    {
        Effect effect = mediator.comboResultUI.SpawnPassResult(notePosition);
        effect.sortingOrder = comboSortingOrder++;
    }

    private void IncreaseFever(ComboPrecision comboPrecision)
    {
        float increase = 0;
        switch (comboPrecision)
        {
            case ComboPrecision.Great:
            increase = 2;
            break;

            case ComboPrecision.Perfect:
            increase = 1;
            break;
        }

        IncreaseFever(increase);
    }

    void EnterToFeverMode()
    {
        fever = true;
        mediator.background.fever.Show();
        mediator.fxAudio.Play(FXA.V_Fever);
    }

    private void IncreaseFever(float increase)
    {
        if (fever)
        {
            return;
        }

        float prevRatio = currentFever / MaxFever;
        currentFever = Mathf.Clamp(currentFever + increase, 0, MaxFever);
        float feverRatio = currentFever / MaxFever;
        mediator.gameUI.SetFever(prevRatio, feverRatio);

        if (feverRatio >= 1.0f)
        {
            EnterToFeverMode();
        }
    }

    void StopFever()
    {
        mediator.background.fever.Hide();
        fever = false;
    }

    void StopFeverImmediate()
    {
        mediator.background.fever.HideImmediate();
        mediator.gameUI.SetFeverImmediate(currentFever / MaxFever);
    }

    void UpdateFeverState()
    {
        if (!fever)
        {
            return;
        }

        currentFever -= Time.deltaTime * (MaxFever / feverDuration);
        currentFever = Mathf.Clamp(currentFever, 0, MaxFever);

        mediator.background.fever.spawnFx = currentFever > 10.0f;

        if (currentFever <= 0)
        {
            StopFever();
        }

        mediator.gameUI.SetFeverImmediate(currentFever / MaxFever);
    }

    float CalculateAccuracyPercent()
    {
        float ratio = sumAccuracy / mediator.music.CalculateMaxAccuracy();
        return ratio * 100.0f;
    }

    Grade CalculateGrade()
    {
        float accuracyPercent = CalculateAccuracyPercent();
        if (accuracyPercent >= 100.0f)
        {
            return Grade.S_Gold;
        }
        else if (accuracyPercent >= 95.0f)
        {
            return Grade.S_Silver;
        }
        else if (accuracyPercent >= 90.0f)
        {
            return Grade.S;
        }
        else if (accuracyPercent >= 80.0f)
        {
            return Grade.A;
        }
        else if (accuracyPercent >= 70.0f)
        {
            return Grade.B;
        }
        else if (accuracyPercent >= 60.0f)
        {
            return Grade.C;
        }
        else
        {
            return Grade.D;
        }
    }

    void ChangeToClearResultScene()
    {
        ClearResultLoader.Desc loadDesc = new ClearResultLoader.Desc();
        ClearResult.Desc desc = new ClearResult.Desc();
        loadDesc.desc = desc;

        desc.accuracy = CalculateAccuracyPercent();
        desc.maxCombo = nMaxCombo;
        desc.perfect = nPerfect;
        desc.great = nGreat;
        desc.pass = nPass;
        desc.miss = nMiss;
        desc.score = (int)score;
        desc.grade = CalculateGrade();
        desc.characterType = mediator.character.characterType;

        ClearResultLoader.Load(loadDesc);
    }

    void ChangeToFailResultScene()
    {
        FailResultLoader.Desc loadDesc = new FailResultLoader.Desc();
        FailResult.Desc desc = new FailResult.Desc();
        loadDesc.desc = desc;

        desc.characterType = mediator.character.characterType;

        FailResultLoader.Load(loadDesc);
    }

    void OnDead()
    {
        mediator.gameUI.Hide();
        mediator.background.dead.ShowAndPlay(0.2f);

        if (coroutine_MusicStop != null)
        {
            StopCoroutine(coroutine_MusicStop);
        }
        coroutine_MusicStop = StartCoroutine(Coroutine_TurnOffMusic(1.0f));
    }

    IEnumerator Coroutine_TurnOffMusic(float duration)
    {
        WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

        float startTime = Time.time;
        float startVolume = mediator.music.volume;
        while (true)
        {
            float t = Mathf.Clamp01((Time.time - startTime) / duration);
            if (duration <= 0.0f)
            {
                t = 1.0f;
            }
            float volume = startVolume * (1 - t);
            mediator.music.volume = volume;

            yield return waitForEndOfFrame;

            if (t >= 1.0f)
            {
                break;
            }
        }

        coroutine_MusicStop = null;
        yield return null;
    }

    void OnStartMusic()
    {
        mediator.gameUI.Show();
    }

    void OnEndPlay()
    {
        ChangeToClearResultScene();
    }

    void OnEndPlayDeadBackground()
    {
        ChangeToFailResultScene();
    }

    private void Awake()
    {
        mediator.music.OnStartMusicWhenGamePlay += OnStartMusic;
        mediator.music.OnEndPlayWhenGamePlay += OnEndPlay;
        mediator.character.OnDeadCallback += OnDead;
        mediator.background.dead.OnEndPlay += OnEndPlayDeadBackground;
    }

    private void Update()
    {
        UpdateFeverState();
    }

    public void GameReset()
    {
        ClearState();
        allowFullCombo = false;
    }
}
