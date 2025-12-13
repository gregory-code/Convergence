using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class LineScript : MonoBehaviour
{
    private LineRenderer lr;

    [SerializeField] private Vector3 startPoint;
    [SerializeField] private GameObject reticle;
    [SerializeField] private GameObject reticleSecondary;

    [SerializeField] private Sprite selectingReticle;
    [SerializeField] private Sprite hoveringCardReticle;

    [SerializeField] private Material dottedMAT;

    private Vector3 previousScale;

    private bool bFocusCaptain;
    private bool bFocusTarget;
    private bool bSelectedCaptain;

    private Transform targetCaptainTransform;
    private Transform targetTransform;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 3;
        FirstEnable(false, new Vector3(0, 0, 0), Color.white);
    }

    private void Start()
    {
        lr.material = dottedMAT;

        previousScale = reticle.transform.localScale;

        //lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
    }

    public void FirstEnable(bool state, Vector3 start, Color theme)
    {
        lr.sortingOrder = -5;
        reticle.SetActive(state);
        reticleSecondary.SetActive(false);
        lr.enabled = state;

        dottedMAT.SetColor("_Theme", theme);
        reticle.GetComponent<SpriteRenderer>().color = theme;
        reticleSecondary.GetComponent<SpriteRenderer>().color = theme;

        reticle.GetComponent<SpriteRenderer>().sprite = selectingReticle;

        bFocusTarget = false;
        bSelectedCaptain = false;

        if (state)
        {
            StartCoroutine(ShowLineRender());
            startPoint = start;
            reticle.transform.position = startPoint;
            lr.SetPosition(0, startPoint);
            lr.SetPosition(1, startPoint);
        }
    }

    public void FocusCaptain(bool state, Transform captain)
    {
        reticle.GetComponent<SpriteRenderer>().sprite = (state) ? hoveringCardReticle : selectingReticle;

        bFocusCaptain = state;
        targetCaptainTransform = captain;
    }
    public bool IsHoveringOverCard()
    {
        if(bFocusCaptain)
            return true;

        if (bFocusTarget)
            return true;

        return false;
    }

    public void SelectCaptain()
    {
        reticle.GetComponent<SpriteRenderer>().sprite = selectingReticle;

        reticleSecondary.transform.position = targetCaptainTransform.position;
        reticleSecondary.SetActive(true);
        bSelectedCaptain = true;
        bFocusCaptain = false;
    }

    public void FocusTarget(bool state, Transform target)
    {
        reticle.GetComponent<SpriteRenderer>().sprite = (state) ? hoveringCardReticle : selectingReticle;

        bFocusTarget = state;
        targetTransform = target;
    }

    private IEnumerator ShowLineRender()
    {
        yield return new WaitForEndOfFrame();
        lr.sortingOrder = 0;
    }

    public void UpdateReticleLocation(Vector3 position)
    {
        reticle.transform.position = Vector3.Lerp(reticle.transform.position, position, 15 * Time.deltaTime);
    }

    private void Update()
    {
        if (lr.enabled == false)
            return;

        Vector3 spot1Pos = (bSelectedCaptain) ? targetCaptainTransform.position : startPoint;
        Vector3 spot2Pos = reticle.transform.position;

        lr.SetPosition(1, spot1Pos);
        lr.SetPosition(2, spot2Pos);

        Vector3 newSize = (bFocusTarget || bFocusCaptain) ? previousScale * 2.0f : previousScale;
        reticle.transform.localScale = Vector3.Lerp(reticle.transform.localScale, newSize, 6 * Time.deltaTime);

        if (bFocusCaptain == true)
        {
            UpdateReticleLocation(targetCaptainTransform.position);
        }

        if (bFocusTarget == true)
        {
            UpdateReticleLocation(targetTransform.position);
        }
    }

}
