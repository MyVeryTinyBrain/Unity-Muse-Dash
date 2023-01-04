using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedScaleInspector : DataListInspector<SpeedScaleData>
{
    protected override void OnUpdateInspector()
    {
        SetList(mediator.music, mediator.music.interfaceSpeedScaleDatas);
    }

    protected override void OnChangeInInspector()
    {
        mediator.music.ApplySpeedScaleDatas();
    }

    protected override void OnClickCreateNewButton()
    {
        SpeedScaleData speedScaleData = mediator.music.interfaceSpeedScaleDatas.Count > 0 ? mediator.music.interfaceSpeedScaleDatas[0] : SpeedScaleData.Default();
        mediator.music.interfaceSpeedScaleDatas.Add(speedScaleData);

        OnChangeInInspector();

        UpdateInspector();
    }

    protected override void OnClickSortButton()
    {
        mediator.music.interfaceSpeedScaleDatas.Sort((SpeedScaleData lhs, SpeedScaleData rhs) => lhs.time.CompareTo(rhs.time));

        UpdateInspector();
    }

    protected override void OnClickDestroyButton(int dataIndex)
    {
        mediator.music.interfaceSpeedScaleDatas.RemoveAt(dataIndex);

        OnChangeInInspector();

        UpdateInspector();
    }

    protected override void OnClickMoveUpButton(int dataIndex)
    {
        int otherIndex = Mathf.Clamp(dataIndex - 1, 0, mediator.music.interfaceSpeedScaleDatas.Count - 1);
        mediator.music.interfaceSpeedScaleDatas.Swap(dataIndex, otherIndex);

        OnChangeInInspector();

        UpdateInspector();
    }

    protected override void OnClickMoveDownButton(int dataIndex)
    {
        int otherIndex = Mathf.Clamp(dataIndex + 1, 0, mediator.music.interfaceSpeedScaleDatas.Count - 1);
        mediator.music.interfaceSpeedScaleDatas.Swap(dataIndex, otherIndex);

        OnChangeInInspector();

        UpdateInspector();
    }
}
