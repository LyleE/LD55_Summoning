using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RuneData
{
    public RuneType Type;
    public bool IsRightRune;
    public Material Material;
    public Material GlowMaterial;
}

[CreateAssetMenu]
public class RuneDatabase : ScriptableObject
{
    [SerializeField]
    private RuneData[] Database;

    private Dictionary<RuneType, RuneData> _lookup;

    public RuneData GetRuneData(RuneType type)
    {
        if (_lookup == null)
        {
            _lookup = new Dictionary<RuneType, RuneData>();
            foreach (var data in Database)
            {
                _lookup.Add(data.Type, data);
            }
        }

        return _lookup[type];
    }
}
