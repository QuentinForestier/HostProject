using System.Collections;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

namespace Host.Toolbox
{
    /// <summary>
    /// Class holding all the functions commun to classes
    /// </summary>
    public class Tools : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            // Make sure that the the current gameObject won't be destroyed
            DontDestroyOnLoad(this);
        }

        /// <summary>
        /// Display a notification
        /// </summary>
        /// <param name="n">NotificationManager component</param>
        /// <param name="title">Notification title</param>
        /// <param name="content">Notificiation content</param>
        public void ShowNotification(NotificationManager n, string title, string content)
        {
            n.title = title;
            n.description = content;
            n.UpdateUI();
            n.OpenNotification();
        }

        /// <summary>
        /// Add a button on a given container
        /// </summary>
        /// <param name="btnPrefab">Prefab of the button to instanciate</param>
        /// <param name="btnName">Name of the button</param>
        /// <param name="container">Container in which the button should be placed</param>
        /// <returns>The newly created GameObject</returns>
        public GameObject AddButtonToContainer(GameObject btnPrefab, string btnName, GameObject container)
        {
            GameObject btn = Instantiate(btnPrefab) as GameObject;
            btn.GetComponentInChildren<TextMeshProUGUI>().SetText(btnName);
            btn.transform.SetParent(container.transform, false);
            return btn;
        }

        /// <summary>
        /// Remove a button with a specific name from the GUI
        /// </summary>
        /// <param name="btnName">Name of the button</param>
        /// <param name="container">Container where the button is</param>
        /// <returns>true if the button has been removed, else false</returns>
        public bool RemoveButtonByName(string btnName, GameObject container)
        {
            for (int i = 0; i < container.transform.childCount; i++)
            {
                string btnText = container.transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text;
                if (btnText == btnName)
                {
                    Destroy(container.transform.GetChild(i).gameObject);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Hide a button
        /// </summary>
        /// <param name="btn">Button GameObject to hide</param>
        public void HideButton(GameObject btn)
        {
            btn.SetActive(false);
        }

        /// <summary>
        /// Show a button
        /// </summary>
        /// <param name="btn">Button GameObject to show</param>
        public void ShowButton(GameObject btn)
        {
            btn.SetActive(true);
        }

        /**
         * 
         * @param container : The parent GameObject
         **/

        /// <summary>
        /// Remove all button from a container
        /// </summary>
        /// <param name="container">Container where the buttons are</param>
        public void RemoveAllButtonFromContainer(GameObject container)
        {
            int numberOfBtn = container.transform.childCount;

            for (int i = 0; i < numberOfBtn; i++)
            {
                Destroy(container.transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Empty a text input
        /// </summary>
        /// <param name="input">Input concerned</param>
        public void EmptyInput(TMP_InputField input)
        {
            input.text = string.Empty;
        }

        /// <summary>
        /// Set all the button of a container to a certain color
        /// </summary>
        /// <param name="container">Container holding the buttons</param>
        /// <param name="c">Color</param>
        public void SetColorAllBtnContainer(GameObject container, Color32 c)
        {
            for (int i = 0; i < container.transform.childCount; i++)
            {
                GameObject btn = container.transform.GetChild(i).gameObject;
                ChangeNormalColorBtn(btn, c);
            }
        }

        /// <summary>
        /// Set the normal color of a button
        /// </summary>
        /// <param name="btn">Button</param>
        /// <param name="c">Color</param>
        public void ChangeNormalColorBtn(GameObject btn, Color32 c)
        {
            Color colors = btn.GetComponent<Image>().color;
            colors = c;
            btn.GetComponent<Image>().color = colors;
        }

        /// <summary>
        /// Change the color of a button when selected
        /// </summary>
        /// <param name="btn">Button</param>
        /// <param name="container">Container in which the button is</param>
        /// <param name="regular">Regular color of the button</param>
        /// <param name="selected">Color of the selected button</param>
        public void ChangeButtonColorOnSelect(GameObject btn, GameObject container, Color32 regular, Color32 selected)
        {
            SetColorAllBtnContainer(container, regular);
            ChangeNormalColorBtn(btn, selected);
        }
    }
}


