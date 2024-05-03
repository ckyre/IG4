using UnityEngine;

public class InterfaceSection : MonoBehaviour
{
    public bool active = false;
    public CanvasGroup canvasGroup;

    private void Awake()
    {
        Active(active);
    }

    public void Active(bool activate)
    {
        active = activate;
        
        if (active == true)
        {
            canvasGroup.alpha = 1.0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            canvasGroup.alpha = 0.0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
