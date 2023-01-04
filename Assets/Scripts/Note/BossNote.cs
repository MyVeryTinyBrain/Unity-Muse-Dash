using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BossNote : Note
{
    protected BossAnimationData bossAnimationData = null;

    public override void OnDestroyInstance()
    {
        base.OnDestroyInstance();

        mediator?.music.DestroyBossAnimationData(bossAnimationData);
    }

    public override void Init()
    {
        base.Init();

        GenerateBossAnimationData();
    }

    public override void Init_Load()
    {
        base.Init_Load();

        bossAnimationData = data.bossAnimationData;
        mediator.music.AddBossAnimationData(bossAnimationData);
    }

    public override void Init_CopyInEditor(Note source)
    {
        base.Init_CopyInEditor(source);

        bossAnimationData = source.data.bossAnimationData.Copy();
        mediator.music.AddBossAnimationData(bossAnimationData);
        data.bossAnimationData = bossAnimationData;
    }

    public override void Init_PasteInEditor(Note source)
    {
        base.Init_PasteInEditor(source);

        Init_CopyInEditor(source);
    }

    public void GenerateBossAnimationData()
    {
        if (bossAnimationData == null)
        {
            bossAnimationData = BossAnimationData.Default();
            mediator.music.AddBossAnimationData(bossAnimationData);
        }

        data.bossAnimationData = bossAnimationData;
    }

    protected override void UpdateBossAnimationData()
    {
        base.UpdateBossAnimationData();

        data.bossAnimationData.noteTime = data.time;
    }
}
