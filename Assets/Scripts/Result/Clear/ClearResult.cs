using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using TMPro;
using UnityEngine.UI;

public class ClearResult : MonoBehaviour
{
    public class Desc
    {
        public float accuracy = 0;
        public int maxCombo = 0;
        public int perfect = 0;
        public int great = 0;
        public int pass = 0;
        public int miss = 0;
        public int score = 0;
        public Grade grade = Grade.S_Gold;
        public CharacterType characterType = CharacterType.None;
    }

    [SerializeField]
    CharacterDescs characterDescs = null;

    [SerializeField]
    SkeletonAnimation characterSpine = null;

    [SerializeField]
    TextMeshPro accuracyText = null;

    [SerializeField]
    TextMeshPro maxComboText = null;

    [SerializeField]
    TextMeshPro perfectText = null;

    [SerializeField]
    TextMeshPro greatText = null;

    [SerializeField]
    TextMeshPro passText = null;

    [SerializeField]
    TextMeshPro missText = null;

    [SerializeField]
    TextMeshPro scoreText = null;

    [SerializeField]
    SpriteRenderer gradeRenderer = null;

    [SerializeField]
    Sprite gradeA = null;

    [SerializeField]
    Sprite gradeB = null;

    [SerializeField]
    Sprite gradeC = null;

    [SerializeField]
    Sprite gradeD = null;

    [SerializeField]
    Sprite gradeS = null;

    [SerializeField]
    Sprite gradeS_Silver = null;

    [SerializeField]
    Sprite gradeS_Gold = null;

    [SerializeField]
    Button restartButton = null;

    [SerializeField]
    Button continueButton = null;

    public static ClearResult Find()
    {
        GameObject gameObject = GameObject.FindGameObjectWithTag(Tags.ClearResult);
        ClearResult component = null;
        gameObject.TryGetComponent<ClearResult>(out component);
        return component;
    }

    public void ApplyDesc(Desc desc)
    {
        // Apply skeleton
        foreach (CharacterDesc characterDesc in characterDescs.descs)
        {
            if (characterDesc.type == desc.characterType)
            {
                characterSpine.Init(characterDesc.clearAsset, characterDesc.clearAssetSkin);
                break;
            }
        }
        characterSpine.AnimationName = "standby";

        // Apply accuracy
        int accuracyInt = (int)desc.accuracy;
        int accuracyDecial = (int)(desc.accuracy * 100.0f) - accuracyInt * 100;
        accuracyText.text = $"{accuracyInt}.{accuracyDecial}%";

        // Apply grade
        Sprite gradeSprite = null;
        switch (desc.grade)
        {
            default:
            case Grade.S_Gold:
            gradeSprite = gradeS_Gold;
            break;

            case Grade.S_Silver:
            gradeSprite = gradeS_Silver;
            break;

            case Grade.S:
            gradeSprite = gradeS;
            break;

            case Grade.A:
            gradeSprite = gradeA;
            break;

            case Grade.B:
            gradeSprite = gradeB;
            break;

            case Grade.C:
            gradeSprite = gradeC;
            break;

            case Grade.D:
            gradeSprite = gradeD;
            break;
        }
        gradeRenderer.sprite = gradeSprite;

        // Apply others
        maxComboText.text = desc.maxCombo.ToString();
        perfectText.text = desc.perfect.ToString();
        greatText.text = desc.great.ToString();
        passText.text = desc.pass.ToString();
        missText.text = desc.miss.ToString();
        scoreText.text = desc.score.ToString();
    }

    void OnClickRestartButton()
    {
        Loader.Load(Loader.GetLastLoadDesc());
    }

    void OnClickContinueButton()
    {
        Loader.Load(Loader.GetRecordedState());
    }

    private void Awake()
    {
        this.gameObject.tag = Tags.ClearResult;
        restartButton.onClick.AddListener(OnClickRestartButton);
        continueButton.onClick.AddListener(OnClickContinueButton);
    }

    private void Update()
    {
        if (Input.GetKeyDown(Keys.ResultRestart))
        {
            OnClickRestartButton();
        }
        else if (Input.GetKeyDown(Keys.ResultContinue))
        {
            OnClickContinueButton();
        }
    }
}
