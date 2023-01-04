using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

public class PlaySettingsPanel : MonoBehaviour
{
    protected Mediator mediator => Mediator.i;

    [SerializeField]
    Button closeButton = null;

    [SerializeField]
    TMP_Dropdown characterDropdown = null;

    [SerializeField]
    TMP_InputField offsetField = null;

    [SerializeField]
    Button clapperToggleButton = null;

    [SerializeField]
    TextMeshProUGUI clapperToggleButtonText = null;

    public void Show()
    {
        this.gameObject.SetActive(true);
        UpdateUI();
    }

    void SetupCharacterOptions()
    {
        List<string> enumNames = Utility.GetNamesOfEnum<CharacterType>();
        characterDropdown.ClearOptions();
        characterDropdown.AddOptions(enumNames);
    }

    void UpdateUI()
    {
        SetupCharacterOptions();
        characterDropdown.SetValueWithoutNotify((int)mediator.character.characterType);
        offsetField.SetTextWithoutNotify(mediator.gameSettings.offset.ToString());
        clapperToggleButtonText.text = mediator.clapper.gameObject.activeSelf ? "Yes" : "No";
    } 

    private void OnClickCloseButton()
    {
        this.gameObject.SetActive(false);
    }

    void OnCharacterDropdownValueChanged(int value)
    {
        CharacterType changed = (CharacterType)Utility.GetEnumOfIndex(typeof(CharacterType), value);
        mediator.character.Init(changed);
    }

    void OnOffsetFieldValueChanged(string value)
    {
        if(float.TryParse(value, out float result))
        {
            mediator.gameSettings.offset = result;
        }
    }

    void OnClapperToggleButtonClick()
    {
        bool active = !mediator.clapper.gameObject.activeSelf;
        mediator.clapper.gameObject.SetActive(active);
        clapperToggleButtonText.text = active ? "Yes" : "No";
    }

    private void Awake()
    {
        closeButton.onClick.AddListener(OnClickCloseButton);
        characterDropdown.onValueChanged.AddListener(OnCharacterDropdownValueChanged);
        offsetField.onValueChanged.AddListener(OnOffsetFieldValueChanged);
        offsetField.contentType = TMP_InputField.ContentType.DecimalNumber;
        clapperToggleButton.onClick.AddListener(OnClapperToggleButtonClick);
    }
}
