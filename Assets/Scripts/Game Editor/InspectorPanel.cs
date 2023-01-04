using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class InspectorPanel : MonoBehaviour
{
    [SerializeField]
    Button closeButton = null;

    [SerializeField]
    TextMeshProUGUI inspectorName = null;

    [SerializeField]
    List<MonoBehaviour> inspectors;

    Dictionary<Type, MonoBehaviour> inspectorByTypes = new Dictionary<Type, MonoBehaviour>();

    public string inspectorLabel
    {
        get => inspectorName.text;
        set => inspectorName.text = value;
    }

    public void ShowInspector(Type type)
    {
        gameObject.SetActive(true);
        inspectors.ForEach((x) => x.gameObject.SetActive(false));

        GameObject targetObject = inspectorByTypes[type].gameObject;
        targetObject.SetActive(true);
        inspectorLabel = targetObject.name;

        int startCut = inspectorLabel.IndexOf('(');
        if (startCut >= 0)
        {
            inspectorLabel = inspectorLabel.Substring(0, startCut);
        }
    }

    private void OnClickCloseButton()
    {
        this.gameObject.SetActive(false);
    }

    private void Awake()
    {
        closeButton.onClick.AddListener(OnClickCloseButton);

        foreach(MonoBehaviour inspector in inspectors)
        {
            inspectorByTypes.Add(inspector.GetType(), inspector);
        }
    }
}
