using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PathString
{
    public string path;

    public PathString(string path)
    {
        this.path = path;
    }

    public override string ToString()
    {
        return path;
    }

    public static implicit operator string(PathString ps)
    {
        return ps.path;
    }

    public static implicit operator PathString(string path)
    {
        return new PathString(path);
    }
}
