using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text.RegularExpressions;

public abstract class EditorField : MonoBehaviour
{
    public delegate void OnChangedDelegate();
    public OnChangedDelegate OnChange;

    [SerializeField]
    TextMeshProUGUI labelText = null;

    public string label
    {
        get => labelText.text;
        set => labelText.text = value;
    }

    public abstract bool isFocused
    {
        get;
    }

    public abstract bool isReadonly
    {
        get; set;
    }
}

public interface IField
{
    public EditorField GetEditorField();

    public void LateUpdate();
    public void Close();
}

public abstract class FieldBase : IField
{
    protected EditorField editorField { get; private set; }

    public FieldBase(EditorField editorField)
    {
        this.editorField = editorField;

        editorField.gameObject.SetActive(true);
    }

    public EditorField GetEditorField()
    {
        return editorField;
    }

    public void LateUpdate()
    {
        if (!editorField.isFocused)
        {
            OnLateUpdate();
        }
    }

    protected abstract void OnLateUpdate();

    public virtual void Close()
    {
        UnityEngine.Object.Destroy(editorField.gameObject);
        editorField = null;
    }

    public string label
    {
        get => editorField.label;
        set => editorField.label = value;
    }
}

public abstract class Field<Class, T> : FieldBase
{
    protected Ref<Class> refRootData;
    protected DepthFieldInfo depth { get; private set; }

    public Field(Ref<Class> refRootData, EditorField editorField, DepthFieldInfo depth) : base(editorField)
    {
        this.refRootData = refRootData;
        this.depth = depth;

        label = depth.fieldInfo.Name;
        if (label.Length > 0 && Regex.IsMatch(label[0].ToString(), @"^[a-zA-Z]+$"))
        {
            label = char.ToUpper(label[0]) + label.Substring(1);
        }
    }

    public override void Close()
    {
        base.Close();

        refRootData = null;
        depth = null;
    }

    public T Value
    {
        get => (T)depth.GetValue(refRootData.Value);
        set => depth.SetValue(refRootData, value);
    }
}
