using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace VAT.DevID
{
    [InitializeOnLoad]
    public static class DeveloperIDFirstPopupEvent
    {
        static DeveloperIDFirstPopupEvent()
        {
            if (DeveloperID.CurrentID == null && DeveloperID.CreateIDLater == false)
            {
                DeveloperIDFirstPopup.OpenWindow();
            }
        }
    }

    public class DeveloperIDFirstPopup : EditorWindow
    {
        internal static void OpenWindow()
        {
            DeveloperIDFirstPopup window = ScriptableObject.CreateInstance<DeveloperIDFirstPopup>();
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 300, 80);
            window.ShowPopup();
        }

        DeveloperIDCreationWindow creationWindow;

        protected void OnGUI()
        {
            GUILayout.Label("Welcome to DevID! Would you like to create your ID...");

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Later?"))
            {
                DeveloperID.MarkCreateLater();
                Close();
            }

            if (GUILayout.Button("Now?")) 
            {
                creationWindow = GetWindow<DeveloperIDCreationWindow>();
                creationWindow.titleContent = new GUIContent("DevID Creation", "Create your DevID here!");
                creationWindow.Show();

                Close();
            }

            GUILayout.EndHorizontal();
        }
    }
}