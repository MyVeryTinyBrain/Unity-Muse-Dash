using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

using ValueType = System.Enum;

public class EnumEditorField : EditorField
{
    public enum Temp { Option_A, Option_B, Option_C };

    [SerializeField]
    TMP_Dropdown dropDown = null;

    public delegate void OnChangeValueDelegate(ValueType prev, ValueType changed);
    public event OnChangeValueDelegate OnChangeValue;

    Type enumType = null;
    ValueType value = null;

    public override bool isFocused
    {
        get => dropDown.IsExpanded;
    }

    public override bool isReadonly
    {
        get => dropDown.interactable;
        set => dropDown.interactable = value;
    }

    public Type GetEnumType()
    {
        return enumType;
    }

    public ValueType GetValue()
    {
        return value;
    }

    public void SetEnumType(Type type)
    {
        if(this.enumType == type)
        {
            return;
        }
        this.enumType = type;

        List<string> enumNames = Utility.GetNamesOfEnum(type);
        dropDown.ClearOptions();
        dropDown.AddOptions(enumNames);

        IEnumerable<Enum> enums = Enum.GetValues(type).Cast<Enum>();
        SetValue(enums.First());
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
            dropDown.SetValueWithoutNotify(Utility.GetIndexOfEnum(enumType, value));

            if (notify)
            {
                OnChangeValue?.Invoke(prev, value);
                OnChange?.Invoke();
            }
        }
    }

    private void Awake()
    {
        dropDown.onValueChanged.AddListener(delegate { OnValueChanged(); });
        SetEnumType(typeof(Temp));
        SetValueWithoutNotify(Temp.Option_A);
    }

    void OnValueChanged()
    {
        ValueType changed = Utility.GetEnumOfIndex(enumType, dropDown.value);
        SetValue(changed);
    }
}

class EnumField<Class> : Field<Class, ValueType>
{
    public EnumField(Ref<Class> refRootData, EnumEditorField editorField, DepthFieldInfo depth) : base(refRootData, editorField, depth)
    {
        editorField.SetEnumType(depth.fieldInfo.FieldType);
        editorField.SetValueWithoutNotify(Value);
        editorField.OnChangeValue += delegate (ValueType prev, ValueType changed)
        {
            if(changed.GetType() == depth.fieldInfo.FieldType)
            {
                Value = changed;
            }
        };
    }

    protected override void OnLateUpdate()
    {
        EnumEditorField enumEditorField = (EnumEditorField)editorField;
        if (enumEditorField.GetEnumType() == depth.fieldInfo.FieldType)
        {
            enumEditorField.SetValueWithoutNotify(Value);
        }
    }
}
