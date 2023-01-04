using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTypeInspector : DataListInspector<MapTypeData>
{
    protected override void OnUpdateInspector()
    {
        SetList(mediator.music, mediator.music.interfaceMapTypeDatas);
    }
    protected override void OnChangeInInspector()
    {
        mediator.music.ApplyMapTypeDatas();
    }

    protected override void OnClickCreateNewButton()
    {
        MapTypeData mapTypeData = mediator.music.interfaceMapTypeDatas.Count > 0 ? mediator.music.interfaceMapTypeDatas[0] : MapTypeData.Default();
        mediator.music.interfaceMapTypeDatas.Add(mapTypeData);
        mediator.music.ApplyMapTypeDatas();

        OnChangeInInspector();

        UpdateInspector();
    }

    protected override void OnClickSortButton()
    {
        mediator.music.interfaceMapTypeDatas.Sort((MapTypeData lhs, MapTypeData rhs) => lhs.time.CompareTo(rhs.time));

        UpdateInspector();
    }

    protected override void OnClickDestroyButton(int dataIndex)
    {
        mediator.music.interfaceMapTypeDatas.RemoveAt(dataIndex);

        OnChangeInInspector();

        UpdateInspector();
    }

    protected override void OnClickMoveUpButton(int dataIndex)
    {
        int otherIndex = Mathf.Clamp(dataIndex - 1, 0, mediator.music.interfaceMapTypeDatas.Count - 1);
        mediator.music.interfaceMapTypeDatas.Swap(dataIndex, otherIndex);

        OnChangeInInspector();

        UpdateInspector();
    }

    protected override void OnClickMoveDownButton(int dataIndex)
    {
        int otherIndex = Mathf.Clamp(dataIndex + 1, 0, mediator.music.interfaceMapTypeDatas.Count - 1);
        mediator.music.interfaceMapTypeDatas.Swap(dataIndex, otherIndex);

        OnChangeInInspector();

        UpdateInspector();
    }
}
