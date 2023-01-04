using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

using ValueType = System.Boolean;

public class BoolEditorField : EditorField
{
    [SerializeField]
    Button button = null;

    [SerializeField]
    TextMeshProUGUI buttonLabel = null;

    public delegate void OnChangeValueDelegate(ValueType prev, ValueType changed);
    public event OnChangeValueDelegate OnChangeValue;

    ValueType value = false;

    public override bool isFocused
    {
        get => false;
    }

    public override bool isReadonly
    {
        get => button.interactable;
        set => button.interactable = value;
    }

    public ValueType GetValue()
    {
        return value;
    }

    public void SetValue(ValueType value)
    {
        SetValueImplementation(value, true);
    }

    public void SetValueWithoutNotify(ValueType value)
    {
        SetValueImplementation(value, false);
    }

    void SetValueImplementation(ValueType value, bool notify)
    {
        ValueType prev = this.value;
        if (prev != value)
        {
            ChangeValue(value);

            if (notify)
            {
                OnChangeValue?.Invoke(prev, value);
                OnChange?.Invoke();
            }
        }
    }

    void ChangeValue(ValueType value)
    {
        this.value = value;

        if (value)
        {
            buttonLabel.text = "Yes";
        }
        else
        {
            buttonLabel.text = "No";
        }
    }

    void ToggleValue()
    {
        SetValue(!value);
    }

    private void Awake()
    {
        button.onClick.AddListener(OnClickButton);
        ChangeValue(false);
    }

    void OnClickButton()
    {
        ToggleValue();
    }
}

class BoolField<Class> : Field<Class, ValueType>
{
    public BoolField(Ref<Class> refRootData, BoolEditorField editorField, DepthFieldInfo depth) : base(refRootData, editorField, depth)
    {
        editorField.SetValueWithoutNotify(Value);
        editorField.OnChangeValue += delegate (ValueType prev, ValueType changed)
        {
            Value = changed;
        };
    }

    protected override void OnLateUpdate()
    {
        ((BoolEditorField)editorField).SetValueWithoutNotify(Value);
    }
}
