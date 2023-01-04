using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITailedNote
{ 
    public float tailTime
    {
        get;
        set;
    }

    public void OnChangeTailSelection(bool selection);
}
