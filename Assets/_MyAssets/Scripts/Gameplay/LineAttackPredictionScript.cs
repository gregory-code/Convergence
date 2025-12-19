using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class LineAttackPredictionScript : MonoBehaviour
{
    private LineRenderer lr;

    [SerializeField] private GameObject reticle;

    [SerializeField] private Material dottedMAT;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
    }

    private void Start()
    {
        lr.material = dottedMAT;

        //lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
    }

    public void ShowPrediction(Vector3 start, Vector3 end, Color theme)
    {
        lr.sortingOrder = -5;
        lr.enabled = true;

        dottedMAT.SetColor("_Theme", theme);
        reticle.GetComponent<SpriteRenderer>().color = theme;

        StartCoroutine(ShowLineRender());

        reticle.transform.position = end;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    private IEnumerator ShowLineRender()
    {
        yield return new WaitForEndOfFrame();
        lr.sortingOrder = 0;
    }
}
