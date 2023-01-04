using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INote
{
    public void SetTime(float time);
    public float GetTime();
    public void SetSelect(bool selected);
}
