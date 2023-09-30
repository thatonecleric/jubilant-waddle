using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnButtonHover : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Color hoverColor;
    public Color originalColor;

    public static HashSet<GameObject> buttons = new HashSet<GameObject>();
    public static string currentlySelectedButton = "";

    private void Start()
    {
        originalColor = text.color;
        buttons.Add(gameObject);
    }

    public void updateTextColorOnHover()
    {
        text.color = hoverColor;
    }

    public void updateTextColorOnExit()
    {
        bool isSelected = currentlySelectedButton == text.text;
        text.color = isSelected ? hoverColor : originalColor;
    }

    public void updateTextColorOnSelect()
    {
        // Highlight and select this color
        text.color = hoverColor;
        currentlySelectedButton = text.text;

        // Unhighlight all other buttons.
        foreach (GameObject obj in buttons)
        {
            obj.GetComponent<OnButtonHover>().updateTextColorOnExit();
        }
    }
}
