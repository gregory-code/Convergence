
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VisualDeck : MonoBehaviour
{
    [SerializeField] GameObject VisualCardBackPrefab;
    private List<GameObject> VisualCardList = new List<GameObject>();

    [SerializeField] private bool bEnemyDeck;

    private Vector3 OriginalPos;

    void Start()
    {
        OriginalPos = transform.position;
    }

    public int VisualDeckAmount() { return VisualCardList.Count; }

    public void AddCardToVisualDeck()
    {
        GameObject newCard = Instantiate(VisualCardBackPrefab, this.transform);
        float randomY = UnityEngine.Random.Range(177f, 184f);

        newCard.transform.rotation = Quaternion.Euler(0f, randomY, 0f);
        if(bEnemyDeck)
        {
            newCard.transform.localPosition = new Vector3(0.0f, 0.005f * VisualCardList.Count, 5.0f);
        }
        else
        {
            newCard.transform.localPosition = new Vector3(0.0f, 0.005f * VisualCardList.Count, -5.0f);
        }

        StartCoroutine(AddCardAnimation(newCard, new Vector3(0.0f, 0.005f * VisualCardList.Count, 0.0f)));

        VisualCardList.Add(newCard);
    }

    public void DrawTopCard()
    {
        StartCoroutine(DrawCardAnimation(VisualCardList[VisualCardList.Count - 1]));
        VisualCardList.RemoveAt(VisualCardList.Count - 1);
    }

    IEnumerator AddCardAnimation(GameObject cardToAdd, Vector3 spot)
    {
        float duration = 1.1f;
        while (duration > 0)
        {
            duration -= Time.deltaTime;
            cardToAdd.transform.localPosition = Vector3.Lerp(cardToAdd.transform.localPosition, spot, 10 * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        cardToAdd.transform.localPosition = spot;
    }

    IEnumerator DrawCardAnimation(GameObject cardToDraw)
    {

        float duration = 1.1f;
        Vector3 newSpot = cardToDraw.transform.localPosition;

        if(bEnemyDeck)
        {
            newSpot.z += 0.3f;
            while (duration > 0)
            {
                if (duration < 0.9f)
                    newSpot.z += 0.01f;

                duration -= Time.deltaTime;
                cardToDraw.transform.localPosition = Vector3.Lerp(cardToDraw.transform.localPosition, newSpot, 10 * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            newSpot.z -= 0.3f;
            while (duration > 0)
            {
                if (duration < 0.9f)
                    newSpot.z -= 0.01f;

                duration -= Time.deltaTime;
                cardToDraw.transform.localPosition = Vector3.Lerp(cardToDraw.transform.localPosition, newSpot, 10 * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
        }


        Destroy(cardToDraw.gameObject);
    }

    public IEnumerator ShuffleAnimation()
    {
        float duration = 0.5f; // shuffle duration
        float elapsed = 0f;

        // Capture original positions and rotations
        Vector3[] startPositions = new Vector3[VisualCardList.Count];
        Quaternion[] startRotations = new Quaternion[VisualCardList.Count];

        for (int i = 0; i < VisualCardList.Count; i++)
        {
            startPositions[i] = VisualCardList[i].transform.localPosition;
            startRotations[i] = VisualCardList[i].transform.localRotation;
        }

        float radius = 0.1f; // radius of the ferris wheel
        Vector3 center = new Vector3(0f, 0f, 0f); // pivot point, adjust as needed

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Sin(elapsed / duration * Mathf.PI); // smooth in/out

            for (int i = 0; i < VisualCardList.Count; i++)
            {
                float angle = (360f / VisualCardList.Count) * i + elapsed * 360f;
                float rad = angle * Mathf.Deg2Rad;

                // Circular motion around the center (Y axis spin)
                float x = Mathf.Cos(rad) * radius;
                float y = Mathf.Sin(rad) * radius;
                float z = 0.005f * i; // keep stacking depth

                VisualCardList[i].transform.localPosition = Vector3.Lerp(
                    startPositions[i],
                    center + new Vector3(x, y, z),
                    t
                );

                // Optional: rotate card to face outward while spinning
                VisualCardList[i].transform.localRotation = Quaternion.Euler(0f, angle, 0f);
            }

            yield return new WaitForEndOfFrame();
        }

        elapsed = 0f;
        duration = 0.5f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            for (int i = 0; i < VisualCardList.Count; i++)
            {
                VisualCardList[i].transform.localPosition = Vector3.Lerp(VisualCardList[i].transform.localPosition, startPositions[i], 5 * Time.deltaTime);
                VisualCardList[i].transform.localRotation = Quaternion.Lerp(VisualCardList[i].transform.localRotation, startRotations[i], 5 * Time.deltaTime);
            }
            yield return new WaitForEndOfFrame();
        }

        // Return to original deck positions
        for (int i = 0; i < VisualCardList.Count; i++)
        {
            VisualCardList[i].transform.localPosition = startPositions[i];
            VisualCardList[i].transform.localRotation = startRotations[i];
        }
    }

}