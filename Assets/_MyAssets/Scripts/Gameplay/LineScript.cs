using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class LineScript : MonoBehaviour
{
    private LineRenderer lr;

    [SerializeField] private Vector3 startPoint;
    [SerializeField] private GameObject reticle;

    [SerializeField] private Material dottedMAT;

    private Vector3 previousScale;

    private bool bFocusTarget;
    private Transform targetTransform;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        enable(false, new Vector3(0, 0, 0));
    }

    private void Start()
    {
        lr.material = dottedMAT;

        previousScale = reticle.transform.localScale;

        //lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
    }

    public void enable(bool state, Vector3 start)
    {
        lr.sortingOrder = -5;
        reticle.SetActive(state);
        lr.enabled = state;

        if (state)
        {
            StartCoroutine(ShowLineRender());
            startPoint = start;
            reticle.transform.position = startPoint;
            lr.SetPosition(0, startPoint);
        }
    }

    public void resetReticle(Vector3 start)
    {
        reticle.transform.position = start;
    }

    public bool IsHoveringOverTarget()
    {
        return bFocusTarget;
    }

    public void UpdateReticleLocation(Vector3 position)
    {
        Vector3 lerp = Vector3.Lerp(reticle.transform.position, position, 15 * Time.deltaTime);
        reticle.transform.position = lerp;
    }

    public void focusTarget(bool state, Transform target)
    {
        bFocusTarget = state;
        targetTransform = target;
    }

    private IEnumerator ShowLineRender()
    {
        yield return new WaitForSeconds(0.1f);
        lr.sortingOrder = 0;
    }

    private void Update()
    {
        if (lr.enabled == false)
            return;

        lr.SetPosition(0, startPoint);
        lr.SetPosition(1, reticle.transform.position);


        Vector3 lerpScale = (bFocusTarget) ? Vector3.Lerp(reticle.transform.localScale, (targetTransform.localScale * 9), 6 * Time.deltaTime)
                                          : Vector3.Lerp(reticle.transform.localScale, previousScale, 6 * Time.deltaTime);
        reticle.transform.localScale = lerpScale;

        if (bFocusTarget == true)
        {
            Vector3 lerpPos = Vector3.Lerp(reticle.transform.position, targetTransform.position, 6 * Time.deltaTime);
            reticle.transform.position = lerpPos;
            return;
        }
    }

}
