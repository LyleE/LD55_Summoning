using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RuneType
{
    None,
    LeftA,
    LeftB,
    LeftC,
    RightA,
    RightB,
    RightC
}

[ExecuteInEditMode]
public class Rune : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer LeftPlane;
    [SerializeField]
    public MeshRenderer RightPlane;

    [SerializeField]
    public RuneDatabase Database;

    public bool Glow;
    public bool Success;
    
    private RuneType _leftRune;
    private RuneType _rightRune;

    public RuneType LeftRune;
    public RuneType RightRune;

    private static System.Random _rand;
    
    void Update()
    {
        if (LeftRune != _leftRune)
        {
            _leftRune = LeftRune;
            var data = Database.GetRuneData(_leftRune);
            LeftPlane.material = Glow? data.GlowMaterial : data.Material;
            if (Glow && Success)
            {
                LeftPlane.material.SetColor("_EmissionColor", Color.yellow);
            }
        }
        
        if (RightRune != _rightRune)
        {
            _rightRune = RightRune;
            var data = Database.GetRuneData(_rightRune);
            RightPlane.material = Glow? data.GlowMaterial : data.Material;
            if (Glow && Success)
            {
                RightPlane.material.SetColor("_EmissionColor", Color.yellow);
            }
        }
    }

    public void Randomise()
    {
        if (_rand == null)
        {
            _rand = new System.Random();
        }
        
        var v = _rand.NextDouble();
        LeftRune = v < 0.333 ? RuneType.LeftA : v < 0.666 ? RuneType.LeftB : RuneType.LeftC;
        v = _rand.NextDouble();
        RightRune = v < 0.333 ? RuneType.RightA : v < 0.666 ? RuneType.RightB : RuneType.RightC;
    }
    
    public void RandomiseNew(RuneType lastLeft, RuneType lastRight)
    {
        if (_rand == null)
        {
            _rand = new System.Random();
        }

        var flip = _rand.NextDouble() < 0.5;
        switch (lastLeft)
        {
            case RuneType.LeftA: LeftRune = flip ? RuneType.LeftB : RuneType.LeftC; break;
            case RuneType.LeftB: LeftRune = flip ? RuneType.LeftC : RuneType.LeftA; break;
            case RuneType.LeftC: LeftRune = flip ? RuneType.LeftA : RuneType.LeftB; break;
        }
        
        flip = _rand.NextDouble() < 0.5;
        switch (lastRight)
        {
            case RuneType.RightA: RightRune = flip ? RuneType.RightB : RuneType.RightC; break;
            case RuneType.RightB: RightRune = flip ? RuneType.RightC : RuneType.RightA; break;
            case RuneType.RightC: RightRune = flip ? RuneType.RightA : RuneType.RightB; break;
        }
    }

    public void ClearLeft()
    {
        LeftRune = RuneType.None;
    }
    
    public void ClearRight()
    {
        RightRune = RuneType.None;
    }

    public void Redraw()
    {
        _leftRune = RuneType.None;
        _rightRune = RuneType.None;
    }
    
    public void SetRune(RuneType rune)
    {
        var data = Database.GetRuneData(rune);
        var plane = data.IsRightRune ? RightPlane : LeftPlane;
        plane.material = Glow? data.GlowMaterial : data.Material;
        if (Glow && Success)
        {
            plane.material.SetColor("_EmissionColor", Color.yellow);
        }

        if (data.IsRightRune)
        {
            _rightRune = rune;
            RightRune = rune;
        }
        else
        {
            _leftRune = rune;
            LeftRune = rune;
        }
    }
}
