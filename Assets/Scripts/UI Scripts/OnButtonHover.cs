using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.VersionControl;
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
        currentlySelectedButton = "";
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
        buttons.RemoveWhere(s => s == null);
        foreach (GameObject obj in buttons)
        {
            if (obj != null) {
                obj.GetComponent<OnButtonHover>().updateTextColorOnExit();
            }
        }
    }
}
