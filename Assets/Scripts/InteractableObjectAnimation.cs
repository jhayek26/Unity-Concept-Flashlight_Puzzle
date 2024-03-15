using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObjectAnimation : MonoBehaviour
{
    float moveTime;

    private void Start()
    {
        moveTime = 0;
    }

    private void Update()
    {
        if (moveTime < 1)
        {
            transform.localPosition = Vector3.down * 0.5f * QuadEaseInOut((moveTime < 0.5f) ? moveTime * 2f : 2 - (moveTime * 2f));
            moveTime += Time.deltaTime / 4;
        }
        else
        {
            moveTime = 0;
        }


        transform.localRotation *= Quaternion.AngleAxis(Time.deltaTime * 60, Vector3.forward);
    }

    //Function based on https://easings.net/#easeInOutQuad
    float QuadEaseInOut(float t)
    {
        return (t < 0.5f) ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
    }
}
