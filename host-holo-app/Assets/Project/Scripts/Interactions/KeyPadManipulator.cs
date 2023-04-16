using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KeyPadManipulator : MonoBehaviour
{
    public List<int> Code;

    private List<int> InputtedCode;

    public int MaxLength = 6;

    public TMPro.TextMeshPro TextCode;

    public UnityEvent OnUnlock;

    private bool isPlayingAnimation = false;

    public void Start()
    {
        KeyPadClear();
    }

    public void KeyPadInput(int number)
    {
        if(InputtedCode == null)
        {
            KeyPadClear();
        }

        if(!isPlayingAnimation)
        {
            InputtedCode.Add(number);
            TextCode.text += number;

            if (InputtedCode.Count >= MaxLength)
            {
                KeyPadValidate();
            }
        }
    }

    public void KeyPadClear()
    {
        InputtedCode = new List<int>();
        TextCode.text = "";
        TextCode.color = new Color(0.4039216f, 0.9529412f, 0f);
        isPlayingAnimation = false;
    }

    public void KeyPadValidate()
    {
        // Compare the two codes
        if(InputtedCode.Count == Code.Count)
        {
            for(int i = 0; i < InputtedCode.Count; i++)
            {
                if(InputtedCode[i] != Code[i])
                {
                    PlayWrongCodeAnimation();
                    return;
                }
            }

            // The code is correct
            PlayCorrectCodeAnimation();
        }
        else
        {
            PlayWrongCodeAnimation();
        }
    }

    private void PlayWrongCodeAnimation()
    {
        isPlayingAnimation = true;

        // Blink code and red LED
        TextCode.color = new Color(0.95f, 0.1f, 0f);

        StartCoroutine(CodeAnimationWrong());


    }

    IEnumerator CodeAnimationWrong()
    {
        for(int i = 0; i < 4; i++)
        {
            yield return new WaitForSeconds(0.4f);
            TextCode.alpha = 0f;

            yield return new WaitForSeconds(0.4f);
            TextCode.alpha = 1f;
        }

        // After a while, go back to empty code and enable input again
        KeyPadClear();
        isPlayingAnimation = false;
    }

    private void PlayCorrectCodeAnimation()
    {
        isPlayingAnimation = true;

        // Blink code and green LED
        StartCoroutine(CodeAnimationCorrect());

    }

    IEnumerator CodeAnimationCorrect()
    {
        for (int i = 0; i < 4; i++)
        {
            yield return new WaitForSeconds(0.4f);
            TextCode.alpha = 0f;

            yield return new WaitForSeconds(0.4f);
            TextCode.alpha = 1f;
        }

        OnUnlock.Invoke();
    }
}
