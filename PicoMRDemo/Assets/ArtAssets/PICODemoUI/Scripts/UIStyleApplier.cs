using UnityEngine;
using TMPro; // For TMP_Text and TMP_InputField
using UnityEngine.UI; // For Button

/// <summary>
/// Helper script to apply default UI style.
/// You may still want to fine tune the UI component later to fit your needs.
/// </summary>
[ExecuteAlways]
public class UIStyleApplier : MonoBehaviour
{
    public enum StyleOption
    {
        Button,
        BodyText,
        BodyTextLarge,
        Display,
        Title,
        Remark,
        InputField,
        Panel,
        TransparentPanel,
    }

    /// <summary>
    /// The UI style data model to use.
    /// </summary>
    public UIStyle uiStyle;
    
    /// <summary>
    /// Options in the UI style.
    /// </summary>
    public StyleOption styleOption;

    private TMP_FontAsset bodyFontAsset => uiStyle.body.fontWeight == TypographyStyle.FontWeight.Prominent700
        ? uiStyle.boldFontAsset
        : uiStyle.fontAsset;
    
    /// <summary>
    /// Method to apply the selected style.
    /// </summary>
    public void ApplyStyle()
    {
        switch (styleOption)
        {
            case StyleOption.Button:
                ApplyButtonStyle();
                break;
            case StyleOption.BodyText:
                ApplyTextStyle(uiStyle.body);
                break;
            case StyleOption.BodyTextLarge:
                ApplyTextStyle(uiStyle.bodyLarge);
                break;
            case StyleOption.Title:
                ApplyTextStyle(uiStyle.title);
                break;
            case StyleOption.Display:
                ApplyTextStyle(uiStyle.display);
                break;
            case StyleOption.Remark:
                ApplyTextStyle(uiStyle.remark);
                break;
            case StyleOption.InputField:
                ApplyInputFieldStyle();
                break;
            case StyleOption.Panel:
                ApplyPanelStyle();
                break;
            case StyleOption.TransparentPanel:
                ApplyTransparentPanelStyle();
                break;
        }
    }

    private void ApplyButtonStyle()
    {
        var buttons = GetComponentsInChildren<Button>();
        foreach (var button in buttons)
        {
            button.image.color = uiStyle.baseHighColor;

            var text = button.GetComponentInChildren<TMP_Text>();
            if (text != null) ApplyTextStyle(uiStyle.body, text);
        }
    }

    private void ApplyTextStyle(TypographyStyle typographyStyle, TMP_Text textComponent = null)
    {
        if (textComponent == null) // If no specific TMP_Text component is provided, apply to all in children
        {
            var texts = GetComponentsInChildren<TMP_Text>();
            foreach (var text in texts)
            {
                SetTextProperties(text, typographyStyle);
            }
        }
        else // Apply to the specific provided TMP_Text component
        {
            SetTextProperties(textComponent, typographyStyle);
        }
    }

    private void ApplyInputFieldStyle()
    {
        var inputFields = GetComponentsInChildren<TMP_InputField>();
        foreach (var inputField in inputFields)
        {
            inputField.fontAsset = bodyFontAsset;
            inputField.SetGlobalPointSize(uiStyle.body.fontSize);
            
            inputField.placeholder.color = uiStyle.textGreyedColor; // Use greyed color for placeholder
            inputField.textComponent.color = uiStyle.textColor; // Use greyed color for placeholder
            
            inputField.transition = Selectable.Transition.None;
        
            // Set the input field's background image color to baseLowColor
            var backgroundImage = inputField.GetComponent<Image>();
            if (backgroundImage != null)
            {
                inputField.image.color = uiStyle.baseLowColor;
            }
        }
    }
    
    private void ApplyPanelStyle()
    {
        var panels = GetComponentsInChildren<Image>(); // Assuming panel is using Image component for background
        foreach (var panel in panels)
        {
            panel.color = uiStyle.baseColor; // Use baseColor for panels
        }
    }
    
    private void ApplyTransparentPanelStyle()
    {
        var panels = GetComponentsInChildren<Image>(); // Assuming panel is using Image component for background
        foreach (var panel in panels)
        {
            panel.color = uiStyle.transparentColor; // Use baseColor for panels
        }
    }

    private void SetTextProperties(TMP_Text text, TypographyStyle typographyStyle)
    {
        text.font = typographyStyle.fontWeight == TypographyStyle.FontWeight.Prominent700 ? uiStyle.boldFontAsset : uiStyle.fontAsset;
        text.fontSize = typographyStyle.fontSize;
        text.color = uiStyle.textColor;
        // Calculate and apply line spacing if needed
    }
}
