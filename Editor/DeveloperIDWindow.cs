using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace VAT.DevID
{
    public class DeveloperIDWindow : EditorWindow
    {
        [MenuItem("VAT/DevID/Management")]
        protected static void OpenWindow()
        {
            DeveloperIDWindow window = GetWindow<DeveloperIDWindow>();
            
            window.titleContent = new GUIContent("DevID Management", "Manage DevID stuff here!");

            window.Show();
        }

        protected DeveloperIDCreationWindow creationWindow;
        protected bool checkValidity = true;
        protected bool idValid = false;

        protected bool hadWindow = false;

        protected void OnGUI()
        {
            if (checkValidity)
            {
                idValid = DeveloperID.CurrentID != null;
            }

            if (GUILayout.Button("Refresh"))
                checkValidity = true;

            if (!idValid)
            {
                if (GUILayout.Button("Create ID"))
                {
                    creationWindow = GetWindow<DeveloperIDCreationWindow>();
                    creationWindow.titleContent = new GUIContent("DevID Creation", "Create your DevID here!");
                    creationWindow.Show();

                    hadWindow = true;
                }
            }
            else
            {
                if (GUILayout.Button("Delete ID"))
                    DeveloperID.DeleteID();

                EditorGUILayout.TextField("User:", DeveloperID.CurrentID.userID);
                EditorGUILayout.TextField("Guid:", DeveloperID.CurrentID.userGuid.ToString());

                GUILayout.Space(10);

                EditorGUILayout.TextField("ID Path:", DeveloperID.PerUserFolderPath);
            }

            if (hadWindow && creationWindow == null)
            {
                checkValidity = true;
                hadWindow = false;
            }
        }
    }
}