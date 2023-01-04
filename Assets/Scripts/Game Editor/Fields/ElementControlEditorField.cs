using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElementControlEditorField : EditorField
{
    public delegate void OnClickDelegate();
    public event OnClickDelegate OnClickDestroy;
    public event OnClickDelegate OnClickMoveUp;
    public event OnClickDelegate OnClickMoveDown;

    [SerializeField]
    Button destroyButton = null;

    [SerializeField]
    Button moveUpButton = null;

    [SerializeField]
    Button moveDownButton = null;

    public override bool isFocused
    {
        get => false;
    }

    public override bool isReadonly
    {
        get => destroyButton.interactable;
        set => destroyButton.interactable = moveUpButton.interactable = moveDownButton.interactable = value;
    }

    void OnClickDestroyButton()
    {
        OnClickDestroy?.Invoke();
    }

    void OnClickMoveUpButton()
    {
        OnClickMoveUp?.Invoke();
    }

    void OnClickMoveDownButton()
    {
        OnClickMoveDown?.Invoke();
    }

    private void Awake()
    {
        destroyButton.onClick.AddListener(OnClickDestroyButton);
        moveUpButton.onClick.AddListener(OnClickMoveUpButton);
        moveDownButton.onClick.AddListener(OnClickMoveDownButton);
    }
}

class ElementControlField : FieldBase
{
    public ElementControlField(ElementControlEditorField editorField) : base(editorField)
    {
    }

    protected override void OnLateUpdate()
    {
    }
}
