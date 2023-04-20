using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trail : MonoBehaviour
{
    public LineRenderer lineRenderer;

    void Start()
    {
        //lineRenderer = GetComponent<LineRenderer>();
    }

    public void Trail(Vector3 pointA, Vector3 pointB, float duration)
    {
        // set the start and end points of the trail
        lineRenderer.SetPosition(0, pointA);
        lineRenderer.SetPosition(1, pointB);
        // get the material of the trail and lerp the alpha from 1 to 0 over the duration
        Material trailMaterial = lineRenderer.sharedMaterial;
        Color trailColor = trailMaterial.color;
        StartCoroutine(LerpAlpha(trailColor, duration));
    }

    private IEnumerator LerpAlpha(Color trailColor, float duration)
    {
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            trailColor.a = Mathf.Lerp(1, 0, elapsedTime / duration);
            lineRenderer.material.color = trailColor;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        trailColor.a = 0;
        lineRenderer.material.color = trailColor;
    }
}
