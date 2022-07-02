using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipGo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// Place this as a componenet on any UI element/2D gameobject to give it the ability to show a tooltip.
    /// Then enter what text you want into its text field.
    /// </summary>

    [Tooltip("Insert what text you want the tooltip to display here")] [TextArea]
    public string tipToShow;
    private float timeToWait = 0.5f;

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(StartTimer());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        Tooltip.OnMouseLoseFocus();
    }

    private void ShowMessage()
    {
        Tooltip.OnMouseHover(tipToShow, Input.mousePosition);
    }

    private IEnumerator StartTimer()
    {
        yield return new WaitForSeconds(timeToWait);

        ShowMessage();
    }
}
