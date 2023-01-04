using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;

public class NoteInspector : MonoBehaviour
{
    [SerializeField]
    LabelEditorField labelEditorFieldTemplate = null;

    [SerializeField]
    BoolEditorField boolEditorFieldTemplate = null;

    [SerializeField]
    IntEditorField intEditorFieldTemplate = null;

    [SerializeField]
    FloatEditorField floatEditorFieldTemplate = null;

    [SerializeField]
    EnumEditorField enumEditorFieldTemplate = null;

    Dictionary<Type, EditorField> editorFieldTemplates = new Dictionary<Type, EditorField>();

    [SerializeField]
    RectTransform content = null;

    List<IField> fields = new List<IField>();

    Note note = null;

    public void Clear()
    {
        note = null;

        fields.ForEach((x) => x.Close());
        fields.Clear();
    }

    T CreateField<T>() where T : EditorField
    {
        EditorField editorField = Instantiate<EditorField>(editorFieldTemplates[typeof(T)], content);
        editorField.gameObject.name = $"{typeof(T).Name} Field";
        return editorField as T;
    }

    public void SetNote(Note note)
    {
        FillTemplates();

        Clear();
        this.note = note;

        List<DepthFieldInfo> depthFieldInfos = FieldAccessAttribute.GetAccessibleFieldDepthInfos(note.data);

        foreach (DepthFieldInfo depth in depthFieldInfos)
        {
            IField field = null;

            if (depth.fieldInfo.FieldType == typeof(bool))
            {
                field = new BoolField<NoteData>(
                    new Ref<NoteData>(() => note.data, v => note.data = v),
                    CreateField<BoolEditorField>(),
                    depth);
            }
            else if (depth.fieldInfo.FieldType == typeof(int))
            {
                field = new IntField<NoteData>(
                    new Ref<NoteData>(() => note.data, v => note.data = v), 
                    CreateField<IntEditorField>(), 
                    depth);
            }
            else if (depth.fieldInfo.FieldType == typeof(float)) 
            {
                field = new FloatField<NoteData>(
                    new Ref<NoteData>(() => note.data, v => note.data = v), 
                    CreateField<FloatEditorField>(), 
                    depth);
            }
            else if (depth.fieldInfo.FieldType.IsEnum)
            {
                field = new EnumField<NoteData>(
                    new Ref<NoteData>(() => note.data, v => note.data = v), 
                    CreateField<EnumEditorField>(), 
                    depth);
            }

            if (field != null)
            {
                fields.Add(field);
            }
        }
    }

    void FillTemplates()
    {
        if (editorFieldTemplates.Count > 0)
        {
            return;
        }

        editorFieldTemplates.Add(labelEditorFieldTemplate.GetType(), labelEditorFieldTemplate);
        editorFieldTemplates.Add(boolEditorFieldTemplate.GetType(), boolEditorFieldTemplate);
        editorFieldTemplates.Add(intEditorFieldTemplate.GetType(), intEditorFieldTemplate);
        editorFieldTemplates.Add(floatEditorFieldTemplate.GetType(), floatEditorFieldTemplate);
        editorFieldTemplates.Add(enumEditorFieldTemplate.GetType(), enumEditorFieldTemplate);
    }

    private void Awake()
    {
        FillTemplates();

        boolEditorFieldTemplate.gameObject.SetActive(false);
        intEditorFieldTemplate.gameObject.SetActive(false);
        floatEditorFieldTemplate.gameObject.SetActive(false);
        enumEditorFieldTemplate.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (note)
        {
            SetNote(note);
        }
    }

    private void Update()
    {
        if (!note)
        {
            Clear();
        }
    }

    private void LateUpdate()
    {
        fields.ForEach((x) => x.LateUpdate());
    }
}
