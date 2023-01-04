using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

using ValueType = System.Single;

public class FloatEditorField : EditorField
{
    [SerializeField]
    TMP_InputField inputField = null;

    public delegate void OnChangeValueDelegate(ValueType prev, ValueType changed);
    public event OnChangeValueDelegate OnChangeValue;

    ValueType value = 0;

    public override bool isFocused
    {
        get => inputField.isFocused;
    }

    public override bool isReadonly
    {
        get => inputField.interactable;
        set => inputField.interactable = value;
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
        this.value = value;
        if (prev != value)
        {
            inputField.SetTextWithoutNotify(value.ToString());

            if (notify)
            {
                OnChangeValue?.Invoke(prev, value);
                OnChange?.Invoke();
            }
        }
    }

    private void Awake()
    {
        inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
        inputField.onEndEdit.AddListener(delegate { OnEndEdit(); });
        inputField.SetTextWithoutNotify(value.ToString());
    }

    void OnEndEdit()
    {
        ValueType changed;
        if (ValueType.TryParse(inputField.text, out changed))
        {
            SetValue(changed);
        }
        else
        {
            inputField.SetTextWithoutNotify(value.ToString());
        }
    }
}

class FloatField<Class> : Field<Class, ValueType>
{
    public FloatField(Ref<Class> refRootData, FloatEditorField editorField, DepthFieldInfo depth) : base(refRootData, editorField, depth)
    {
        editorField.SetValueWithoutNotify(Value);
        editorField.OnChangeValue += delegate (ValueType prev, ValueType changed)
        {
            Value = changed;
        };
    }

    protected override void OnLateUpdate()
    {
        ((FloatEditorField)editorField).SetValueWithoutNotify(Value);
    }
}
