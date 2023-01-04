using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuButton : MonoBehaviour
{
    public delegate void OnClick(PauseMenuButton button);
    public event OnClick onClick;

    [SerializeField]
    Button button = null;

    [SerializeField]
    Image selectImage = null;

    public void SetSelect(bool value)
    {
        selectImage.gameObject.SetActive(value);
    }

    void OnButtonClick()
    {
        onClick?.Invoke(this);
    }

    private void Awake()
    {
        button.onClick.AddListener(OnButtonClick);
    }
}
