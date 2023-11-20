using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Spawner : MonoBehaviour
{
    public Color element = Color.blue;

    

    private void OnEnable()
    {
        //GetComponent<MeshRenderer>().material.color = element;

        GetComponent<MeshRenderer>().sharedMaterial.color = element;
        

        Trap.spawners.Add(this);
    }

    private void OnDisable()
    {
        Trap.spawners.Remove(this);
    }
}
