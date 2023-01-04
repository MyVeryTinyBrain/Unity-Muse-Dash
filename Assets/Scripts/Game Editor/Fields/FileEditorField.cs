using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using SimpleFileBrowser;
using System.IO;

using ValueType = PathString;

public class FileEditorField : EditorField
{
    [System.Serializable]
    public struct Filter
    {
        public string name;
        public List<string> extensions;

        public Filter(string name, string extension)
        {
            this.name = name;
            this.extensions = new List<string>() { extension };
        }

        public Filter(string name, params string[] extensions)
        {
            this.name = name;
            this.extensions = new List<string>(extensions);
        }

        public Filter(string name, List<string> extensions)
        {
            this.name = name;
            this.extensions = new List<string>(extensions);
        }

        public FileBrowser.Filter ToFileBrowserFilter()
        {
            return new FileBrowser.Filter(name, extensions.ToArray());
        }
    }

    [SerializeField]
    Button browseButton = null;

    [SerializeField]
    TMP_InputField readonlyInputField = null;

    public string initialPath = "C:\\Users\\[YourPath]";
    public string initialFilename = "myFile.txt";
    public List<Filter> filters = new List<Filter>()
    {
        new Filter("Text files", ".txt"),
    };
    public string defaultFilter = ".txt";

    public bool useDirectory = true;
    public bool useFileName = true;
    public bool useExtension = true;

    public delegate void OnChangeValueDelegate(ValueType prevPath, ValueType changedPath);
    public delegate void OnChangeValueExDelegate(ValueType prevPath, ValueType changedPath, ValueType prevFullPath, ValueType changedFullPath);
    public event OnChangeValueDelegate OnChangeValue;
    public event OnChangeValueExDelegate OnChangeValueEx;

    ValueType currentPath = "";
    ValueType currentFullPath = "";

    public override bool isFocused
    {
        get => readonlyInputField.isFocused;
    }

    public override bool isReadonly
    {
        get => browseButton.interactable;
        set => browseButton.interactable = value;
    }

    public ValueType GetValue()
    {
        return currentPath;
    }

    public void SetValue(ValueType fullPath)
    {
        SetValueImplementation(fullPath, true);
    }

    public void SetValueWithoutNotify(ValueType fullPath)
    {
        SetValueImplementation(fullPath, false);
    }

    void SetValueImplementation(ValueType fullPath, bool notify)
    {
        ValueType prevPath = currentPath;
        ValueType prevFullPath = currentFullPath;

        if (prevFullPath != fullPath)
        {
            string directory = Path.GetDirectoryName(fullPath);
            string name = Path.GetFileNameWithoutExtension(fullPath);
            string extension = Path.GetExtension(fullPath);
            string path = "";

            if (useDirectory)
            {
                path += directory;
            }

            if (useFileName)
            {
                path += name;
            }

            if (useExtension)
            {
                path += extension;
            }

            currentPath = path;
            currentFullPath = fullPath;

            readonlyInputField.SetTextWithoutNotify(path);

            if (notify)
            {
                OnChangeValue?.Invoke(prevPath, path);
                OnChangeValueEx?.Invoke(prevPath, path, prevFullPath, currentFullPath);
                OnChange?.Invoke();
            }
        }
    }

    private void Awake()
    {
        readonlyInputField.contentType = TMP_InputField.ContentType.Standard;
        readonlyInputField.readOnly = true;
        readonlyInputField.onEndEdit.AddListener(delegate { OnEndEdit(); });
        readonlyInputField.SetTextWithoutNotify(currentPath);

        browseButton.onClick.AddListener(OnClickBrowseButton);
    }

    void OnEndEdit()
    {
        SetValue(readonlyInputField.text);
    }

    void OnClickBrowseButton()
    {
        List<FileBrowser.Filter> fbFilters = new List<FileBrowser.Filter>();
        filters.ForEach((x) => fbFilters.Add(x.ToFileBrowserFilter()));

        FileBrowser.SetFilters(false, fbFilters.ToArray());
        FileBrowser.SetDefaultFilter(defaultFilter);

        FileBrowser.ShowLoadDialog(OnSelectFile, null, FileBrowser.PickMode.Files, false, initialPath, initialFilename, "Load", "Select");
    }

    void OnSelectFile(string[] paths)
    {
        SetValue(paths[0]);
    }
}

class FileField<Class> : Field<Class, ValueType>
{
    public delegate void OnSelectDelegate(string prevPath, string changedPath, string prevFullPath, string changedFullPath, DepthFieldInfo depth);
    public event OnSelectDelegate OnSelect;

    public FileField(Ref<Class> refRootData, FileEditorField editorField, DepthFieldInfo depth) : base(refRootData, editorField, depth)
    {
        editorField.SetValueWithoutNotify(Value);
        editorField.OnChangeValueEx += delegate (ValueType prevPath, ValueType changedPath, ValueType prevFullPath, ValueType changedFullPath)
        {
            Value = changedPath;
            OnSelect?.Invoke(prevPath, changedPath, prevFullPath, changedFullPath, depth);
        };
    }

    protected override void OnLateUpdate()
    {
        ((FileEditorField)editorField).SetValueWithoutNotify(Value);
    }
}
