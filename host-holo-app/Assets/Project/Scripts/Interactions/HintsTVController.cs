using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintsTVController : MonoBehaviour
{
    [Title("References")]
    public TMPro.TextMeshProUGUI Message;
    public Image Icon;

    public GameObject Background;
    public GameObject Hint;

    public GameObject HelpImageContainer;
    public Image HelpImage;

    public AudioSource AnnoucementSound;

    [Title("Icons")]
    public Sprite IconInfo;
    public Sprite IconWarning;
    public Sprite IconError;
    public Sprite IconHorn;

    public enum IconType
    {
        Info,
        Warning,
        Error,
        Horn
    }    

    private Sprite GetIcon(IconType type)
    {
        switch (type)
        {
            case IconType.Info:
                return IconInfo;
            case IconType.Warning:
                return IconWarning;
            case IconType.Error:
                return IconError;
            case IconType.Horn:
                return IconHorn;
        }

        // By default return IconInfo
        return IconInfo;
    }

    public void Start()
    {
        HideMessage();
        HideImage();
    }

    public void ShowImage(Sprite image, float duration = 50)
    {
        HelpImage.sprite = image;

        Hint.SetActive(false);
        HelpImageContainer.SetActive(true);
        Background.SetActive(false);

        AnnoucementSound.Play();

        // Cancel any existing invoke calls
        CancelInvoke("HideImage");

        Invoke("HideImage", duration);
    }

    public void HideImage()
    {
        HelpImageContainer.SetActive(false);
        Background.SetActive(true);
    }

    /// <summary>
    /// Displays a message on the screen for the selected duration
    /// </summary>
    /// <param name="icon">Which icon should be displayed</param>
    /// <param name="message">The message content</param>
    /// <param name="time">The duration of the message in seconds</param>
    public void ShowMessage(IconType icon, string message, float duration = 50)
    {
        Message.text = message;
        Icon.sprite = GetIcon(icon);

        HelpImageContainer.SetActive(false);
        Hint.SetActive(true);
        Background.SetActive(false);

        AnnoucementSound.Play();

        // Cancel any existing invoke calls
        CancelInvoke("HideMessage");

        Invoke("HideMessage", duration);
    }

    /// <summary>
    /// Hides the message and show the background
    /// </summary>
    public void HideMessage()
    {
        Hint.SetActive(false);
        Background.SetActive(true);
    }
}
