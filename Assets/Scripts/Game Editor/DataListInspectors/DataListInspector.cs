using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine.UI;

public abstract class DataListInspector<T> : MonoBehaviour
{
    [SerializeField]
    ElementControlEditorField elementControlEditorFieldTemplate = null;

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

    Dictionary<Type, EditorField> editorFieldTemplates = null;

    [SerializeField]
    RectTransform content = null;

    [SerializeField]
    Button createNewButton = null;

    [SerializeField]
    Button sortButton = null;

    List<IField> fields = new List<IField>();

    object parentClass = null;
    List<T> dataList = null;

    protected Mediator mediator => Mediator.i;

    public void Clear()
    {
        fields.ForEach((x) => x.Close());
        fields.Clear();
    }

    EditorFieldType CreateField<EditorFieldType>() where EditorFieldType : EditorField
    {
        EditorField editorField = Instantiate<EditorField>(editorFieldTemplates[typeof(EditorFieldType)], content);
        editorField.gameObject.name = $"{typeof(EditorFieldType).Name} Field";
        return editorField as EditorFieldType;
    }

    public void SetList(object parentClass, List<T> list)
    {
        this.parentClass = parentClass;
        this.dataList = list;

        Clear();

        for (int i = 0; i < dataList.Count; i++)
        {
            int capture_i = i;

            List<DepthFieldInfo> depthFieldInfos = FieldAccessAttribute.GetAccessibleFieldDepthInfos(dataList[i]);

            // 각 타입의 맨 위에 요소 컨트롤 필드를 부착합니다.
            if (depthFieldInfos.Count > 0)
            {
                ElementControlEditorField elementControlEditorField = CreateField<ElementControlEditorField>();
                elementControlEditorField.OnClickDestroy += () => OnClickDestroyButton(capture_i);
                elementControlEditorField.OnClickMoveUp += () => OnClickMoveUpButton(capture_i);
                elementControlEditorField.OnClickMoveDown += () => OnClickMoveDownButton(capture_i);
                fields.Add(new ElementControlField(elementControlEditorField));
            }

            foreach (DepthFieldInfo depth in depthFieldInfos)
            {
                IField field = null;

                if (depth.fieldInfo.FieldType == typeof(bool))
                {
                    field = new BoolField<T>(
                        new Ref<T>(() => dataList[capture_i], v => dataList[capture_i] = v),
                        CreateField<BoolEditorField>(),
                        depth);
                }
                else if (depth.fieldInfo.FieldType == typeof(int))
                {
                    field = new IntField<T>(
                        new Ref<T>(() => dataList[capture_i], v => dataList[capture_i] = v),
                        CreateField<IntEditorField>(),
                        depth);
                }
                else if (depth.fieldInfo.FieldType == typeof(float))
                {
                    field = new FloatField<T>(
                        new Ref<T>(() => dataList[capture_i], v => dataList[capture_i] = v),
                        CreateField<FloatEditorField>(),
                        depth);
                }
                else if (depth.fieldInfo.FieldType.IsEnum)
                {
                    field = new EnumField<T>(
                        new Ref<T>(() => dataList[capture_i], v => dataList[capture_i] = v),
                        CreateField<EnumEditorField>(),
                        depth);
                }

                if (field != null)
                {
                    field.GetEditorField().OnChange += OnChangeInInspector;

                    fields.Add(field);
                }
            }

            // 마지막 요소가 아닐 때, 각 요소를 빈 칸으로 구분합니다.
            if(i < dataList.Count - 1 && depthFieldInfos.Count > 0)
            {
                fields.Add(new LabelField(CreateField<LabelEditorField>(), ""));
            }
        }
    }

    public void UpdateInspector()
    {
        OnUpdateInspector();
    }

    protected abstract void OnUpdateInspector();
    protected abstract void OnChangeInInspector();
    protected abstract void OnClickCreateNewButton();
    protected abstract void OnClickSortButton();
    protected abstract void OnClickDestroyButton(int dataIndex);
    protected abstract void OnClickMoveUpButton(int dataIndex);
    protected abstract void OnClickMoveDownButton(int dataIndex);

    private void Awake()
    {
        editorFieldTemplates = new Dictionary<Type, EditorField>();
        editorFieldTemplates.Add(elementControlEditorFieldTemplate.GetType(), elementControlEditorFieldTemplate);
        editorFieldTemplates.Add(labelEditorFieldTemplate.GetType(), labelEditorFieldTemplate);
        editorFieldTemplates.Add(boolEditorFieldTemplate.GetType(), boolEditorFieldTemplate);
        editorFieldTemplates.Add(intEditorFieldTemplate.GetType(), intEditorFieldTemplate);
        editorFieldTemplates.Add(floatEditorFieldTemplate.GetType(), floatEditorFieldTemplate);
        editorFieldTemplates.Add(enumEditorFieldTemplate.GetType(), enumEditorFieldTemplate);

        boolEditorFieldTemplate.gameObject.SetActive(false);
        intEditorFieldTemplate.gameObject.SetActive(false);
        floatEditorFieldTemplate.gameObject.SetActive(false);
        enumEditorFieldTemplate.gameObject.SetActive(false);

        createNewButton.onClick.AddListener(OnClickCreateNewButton);
        sortButton.onClick.AddListener(OnClickSortButton);
    }

    private void LateUpdate()
    {
        fields.ForEach((x) => x.LateUpdate());
    }

    private void OnEnable()
    {
        UpdateInspector();
    }
}
