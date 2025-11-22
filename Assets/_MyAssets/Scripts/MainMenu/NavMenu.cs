using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using TMPro;

public class NavMenu : MonoBehaviour
{
    [SerializeField] Image[] MenuIcons;
    [SerializeField] Color[] MenuColors;

    [SerializeField] CanvasGroup[] MenuGroups;

    [SerializeField] Transform[] MenuTexts;

    [SerializeField] float X_Over;
    [SerializeField] float X_Default;
    public void OpenMenu(int id)
    {
        StopAllCoroutines();
        StartCoroutine(MoveMenus(id));
    }

    private IEnumerator MoveMenus(int menuIndex)
    {
        float time = 0.1f;
        while (time > 0)
        {
            yield return new WaitForEndOfFrame();
            time -= Time.deltaTime;

            for (int i = 0; i < MenuIcons.Length; i++)
            {
                if (menuIndex == i)
                {
                    Vector3 overPos = new Vector3(X_Over, MenuIcons[i].transform.position.y, 0);
                    MenuIcons[i].transform.position = Vector3.Lerp(MenuIcons[i].transform.position, overPos, 50 * Time.deltaTime);
                    MenuIcons[i].color = Color.Lerp(MenuIcons[i].color, MenuColors[i], 30 * Time.deltaTime);
                    MenuGroups[i].alpha = Mathf.Lerp(MenuGroups[i].alpha, 1, 30 * Time.deltaTime);
                    MenuTexts[i].localScale = Vector3.Lerp(MenuTexts[i].localScale, Vector3.one, 30 * Time.deltaTime);
                    continue;
                }

                Vector3 newPos = new Vector3(X_Default, MenuIcons[i].transform.position.y, 0);
                MenuIcons[i].transform.position = Vector3.Lerp(MenuIcons[i].transform.position, newPos, 50 * Time.deltaTime);
                MenuIcons[i].color = Color.Lerp(MenuIcons[i].color, Color.white, 30 * Time.deltaTime);
                MenuGroups[i].alpha = Mathf.Lerp(MenuGroups[i].alpha, 0, 30 * Time.deltaTime);
                MenuTexts[i].localScale = Vector3.Lerp(MenuTexts[i].localScale, Vector3.zero, 30 * Time.deltaTime);
            }
        }

        for (int i = 0; i < MenuIcons.Length; i++)
        {
            MenuTexts[i].localScale = (menuIndex == i) ? Vector3.one : Vector3.zero;
            MenuGroups[i].interactable = (menuIndex == i) ? true : false;
            MenuGroups[i].blocksRaycasts = (menuIndex == i) ? true : false;
            MenuGroups[i].alpha = (menuIndex == i) ? 1 : 0;
        }

    }
}
