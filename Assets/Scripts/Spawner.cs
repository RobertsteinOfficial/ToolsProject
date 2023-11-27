using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Spawner : MonoBehaviour
{
    public Color element = Color.blue;

    MaterialPropertyBlock mpb;

    public MaterialPropertyBlock Mpb
    {
        get
        {
            if (mpb == null)
            {
                mpb = new MaterialPropertyBlock();
            }
            return mpb;
        }
    }

    static readonly int propCol = Shader.PropertyToID("_Color");

    private void OnValidate()
    {
        ApplyColour();
    }

    private void OnEnable()
    {
        //GetComponent<MeshRenderer>().material.color = element;


        //Shader shader = Shader.Find("Default/Diffuse");
        //Material mat = new Material(shader);
        //mat.hideFlags = HideFlags.HideAndDontSave;


        Trap.spawners.Add(this);

    }

    private void OnDisable()
    {
        Trap.spawners.Remove(this);
    }

    public void ApplyColour()
    {
        MeshRenderer rnd = GetComponent<MeshRenderer>();


        Mpb.SetColor(propCol, element);
        rnd.SetPropertyBlock(Mpb);
    }

    public void ApplyColour(Color col)
    {
        MeshRenderer rnd = GetComponent<MeshRenderer>();


        Mpb.SetColor(propCol, col);
        rnd.SetPropertyBlock(Mpb);
    }
}
