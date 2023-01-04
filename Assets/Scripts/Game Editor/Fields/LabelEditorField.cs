using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelEditorField : EditorField
{
    public override bool isFocused
    {
        get => false;
    }

    public override bool isReadonly
    {
        get => true;
        set { }
    }
}

class LabelField : FieldBase
{
    public LabelField(LabelEditorField editorField, string labelText = "") : base(editorField)
    {
        label = labelText;
    }

    protected override void OnLateUpdate()
    {
    }
}
