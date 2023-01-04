using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using UnityEngine.UI;

public class FailResult : MonoBehaviour
{
    public class Desc
    {
        public CharacterType characterType = CharacterType.None;
    }

    [SerializeField]
    CharacterDescs characterDescs = null;

    [SerializeField]
    SkeletonGraphic characterSpine = null;

    [SerializeField]
    Button backButton = null;

    [SerializeField]
    Button restartButton = null;

    public static FailResult Find()
    {
        GameObject gameObject = GameObject.FindGameObjectWithTag(Tags.FailResult);
        FailResult component = null;
        gameObject.TryGetComponent<FailResult>(out component);
        return component;
    }

    public void ApplyDesc(Desc desc)
    {
        // Apply skeleton
        foreach (CharacterDesc characterDesc in characterDescs.descs)
        {
            if (characterDesc.type == desc.characterType)
            {
                characterSpine.Init(characterDesc.failAsset, characterDesc.failAssetSkin);
                break;
            }
        }
        characterSpine.AnimationState.AddAnimation(0, "standby", true, 0);
    }

    void OnClickBackButton()
    {
        Loader.Load(Loader.GetRecordedState());
    }

    void OnClickRestartButton()
    {
        Loader.Load(Loader.GetLastLoadDesc());
    }

    private void Awake()
    {
        this.gameObject.tag = Tags.FailResult;

        backButton.onClick.AddListener(OnClickBackButton);
        restartButton.onClick.AddListener(OnClickRestartButton);
    }
}
