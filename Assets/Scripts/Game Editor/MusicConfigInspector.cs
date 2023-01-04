using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MusicConfigInspector : MonoBehaviour
{
    protected Mediator mediator => Mediator.i;

    [SerializeField]
    BoolEditorField boolEditorFieldTemplate = null;

    [SerializeField]
    IntEditorField intEditorFieldTemplate = null;

    [SerializeField]
    FloatEditorField floatEditorFieldTemplate = null;

    [SerializeField]
    EnumEditorField enumEditorFieldTemplate = null;

    [SerializeField]
    StringEditorField stringEditorFieldTemplate = null;

    [SerializeField]
    FileEditorField fileEditorFieldTemplate = null;

    Dictionary<Type, EditorField> editorFieldTemplates = null;

    [SerializeField]
    RectTransform content = null;

    List<IField> fields = new List<IField>();

    public void Clear()
    { 
        fields.ForEach((x) => x.Close());
        fields.Clear();
    }

    T CreateField<T>() where T : EditorField
    {
        EditorField editorField = Instantiate<EditorField>(editorFieldTemplates[typeof(T)], content);
        editorField.gameObject.name = $"{typeof(T).Name} Field";
        return editorField as T;
    }

    public void UpdateInspector()
    {
        Clear();
        Music music = mediator.music;

        List<DepthFieldInfo> depthFieldInfos = FieldAccessAttribute.GetAccessibleFieldDepthInfos(music.musicConfigData);

        foreach (DepthFieldInfo depth in depthFieldInfos)
        {
            IField field = null;

            if (depth.fieldInfo.FieldType == typeof(bool))
            {
                field = new BoolField<MusicConfigData>(
                    new Ref<MusicConfigData>(() => music.musicConfigData, v => music.musicConfigData = v),
                    CreateField<BoolEditorField>(),
                    depth);
            }
            else if (depth.fieldInfo.FieldType == typeof(int))
            {
                field = new IntField<MusicConfigData>(
                    new Ref<MusicConfigData>(() => music.musicConfigData, v => music.musicConfigData = v),
                    CreateField<IntEditorField>(),
                    depth);
            }
            else if (depth.fieldInfo.FieldType == typeof(float))
            {
                field = new FloatField<MusicConfigData>(
                    new Ref<MusicConfigData>(() => music.musicConfigData, v => music.musicConfigData = v),
                    CreateField<FloatEditorField>(),
                    depth);
            }
            else if (depth.fieldInfo.FieldType.IsEnum)
            {
                field = new EnumField<MusicConfigData>(
                    new Ref<MusicConfigData>(() => music.musicConfigData, v => music.musicConfigData = v),
                    CreateField<EnumEditorField>(),
                    depth);
            }
            else if (depth.fieldInfo.FieldType == typeof(string))
            {
                field = new StringField<MusicConfigData>(
                    new Ref<MusicConfigData>(() => music.musicConfigData, v => music.musicConfigData = v),
                    CreateField<StringEditorField>(),
                    depth);
            }
            else if (depth.fieldInfo.FieldType == typeof(PathString))
            {
                FileField<MusicConfigData> fileField = new FileField<MusicConfigData>(
                    new Ref<MusicConfigData>(() => music.musicConfigData, v => music.musicConfigData = v),
                    CreateField<FileEditorField>(),
                    depth);

                fileField.OnSelect += OnChangeFileField;

                field = fileField;
            }

            if (field != null)
            {
                fields.Add(field);
            }
        }
    }

    void OnChangeFileField(string prevPath, string changedPath, string prevFullPath, string changedFullPath, DepthFieldInfo depth)
    {
        if(depth.fieldInfo.Name == nameof(MusicConfigData.musicFileName))
        {
            mediator.music.LoadClip(changedFullPath);
        }
    }

    private void Awake()
    {
        editorFieldTemplates = new Dictionary<Type, EditorField>();
        editorFieldTemplates.Add(boolEditorFieldTemplate.GetType(), boolEditorFieldTemplate);
        editorFieldTemplates.Add(intEditorFieldTemplate.GetType(), intEditorFieldTemplate);
        editorFieldTemplates.Add(floatEditorFieldTemplate.GetType(), floatEditorFieldTemplate);
        editorFieldTemplates.Add(enumEditorFieldTemplate.GetType(), enumEditorFieldTemplate);
        editorFieldTemplates.Add(stringEditorFieldTemplate.GetType(), stringEditorFieldTemplate);
        editorFieldTemplates.Add(fileEditorFieldTemplate.GetType(), fileEditorFieldTemplate);

        boolEditorFieldTemplate.gameObject.SetActive(false);
        intEditorFieldTemplate.gameObject.SetActive(false);
        floatEditorFieldTemplate.gameObject.SetActive(false);
        enumEditorFieldTemplate.gameObject.SetActive(false);
        stringEditorFieldTemplate.gameObject.SetActive(false);
        fileEditorFieldTemplate.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        UpdateInspector();
    }

    private void LateUpdate()
    {
        fields.ForEach((x) => x.LateUpdate());
    }
}
