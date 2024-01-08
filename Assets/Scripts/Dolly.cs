using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DollyMode
{
    Once,
    Loop,
    PingPong
}

public class Dolly : MonoBehaviour
{
    public DollyMode mode;
    public Spline spline;
    public float duration;

    private float progress;

    private int forward = 1;


    private void Update()
    {

        progress += Time.deltaTime / duration * forward;

        if (progress > 1)
        {
            if (mode == DollyMode.Once)
                progress = 1;
            else if (mode == DollyMode.Loop)
                progress -= 1;
            else
            {
                progress = 2f - progress;
                forward = -1;
            }
        }
        else if (progress < 0)
        {
            forward = 1;
            progress = -progress;
        }

        transform.position = spline.GetPoint(progress);
        transform.LookAt(transform.position + spline.GetDirection(progress));

    }

    [ContextMenu("MyMethod")]
    public void MyMethod()
    {

    }

}
