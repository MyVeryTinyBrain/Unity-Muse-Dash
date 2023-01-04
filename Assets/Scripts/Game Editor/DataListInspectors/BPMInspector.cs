using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPMInspector : DataListInspector<BPMData>
{
    protected override void OnUpdateInspector()
    {
        SetList(mediator.music, mediator.music.interfaceBPMDatas);
    }

    protected override void OnChangeInInspector()
    {
        mediator.music.ApplyBPMDatas();
    }

    protected override void OnClickCreateNewButton()
    {
        BPMData bpmData = mediator.music.interfaceBPMDatas.Count > 0 ? mediator.music.interfaceBPMDatas[0] : BPMData.Default();
        mediator.music.interfaceBPMDatas.Add(bpmData);

        OnChangeInInspector();

        UpdateInspector();
    }

    protected override void OnClickSortButton()
    {
        mediator.music.interfaceBPMDatas.Sort((BPMData lhs, BPMData rhs) => lhs.time.CompareTo(rhs.time));

        UpdateInspector();
    }

    protected override void OnClickDestroyButton(int dataIndex)
    {
        mediator.music.interfaceBPMDatas.RemoveAt(dataIndex);

        OnChangeInInspector();

        UpdateInspector();
    }

    protected override void OnClickMoveUpButton(int dataIndex)
    {
        int otherIndex = Mathf.Clamp(dataIndex - 1, 0, mediator.music.interfaceBPMDatas.Count - 1);
        mediator.music.interfaceBPMDatas.Swap(dataIndex, otherIndex);

        OnChangeInInspector();

        UpdateInspector();
    }

    protected override void OnClickMoveDownButton(int dataIndex)
    {
        int otherIndex = Mathf.Clamp(dataIndex + 1, 0, mediator.music.interfaceBPMDatas.Count - 1);
        mediator.music.interfaceBPMDatas.Swap(dataIndex, otherIndex);

        OnChangeInInspector();

        UpdateInspector();
    }
}
