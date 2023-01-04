using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

using ValueType = System.String;

public class StringEditorField : EditorField
{
    [SerializeField]
    TMP_InputField inputField = null;

    public delegate void OnChangeValueDelegate(ValueType prev, ValueType changed);
    public event OnChangeValueDelegate OnChangeValue;

    ValueType value = "";

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
            inputField.SetTextWithoutNotify(value);

            if (notify)
            {
                OnChangeValue?.Invoke(prev, value);
                OnChange?.Invoke();
            }
        }
    }

    private void Awake()
    {
        inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        inputField.onEndEdit.AddListener(delegate { OnEndEdit(); });
        inputField.SetTextWithoutNotify(value);
    }

    void OnEndEdit()
    {
        SetValue(inputField.text);
    }
}

class StringField<Class> : Field<Class, ValueType>
{
    public StringField(Ref<Class> refRootData, StringEditorField editorField, DepthFieldInfo depth) : base(refRootData, editorField, depth)
    {
        editorField.SetValueWithoutNotify(Value);
        editorField.OnChangeValue += delegate (ValueType prev, ValueType changed)
        {
            Value = changed;
        };
    }

    protected override void OnLateUpdate()
    {
        ((StringEditorField)editorField).SetValueWithoutNotify(Value);
    }
}
