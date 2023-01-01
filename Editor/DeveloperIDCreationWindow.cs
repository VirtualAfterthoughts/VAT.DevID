using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace VAT.DevID
{
    public class DeveloperIDCreationWindow : EditorWindow
    {
        public string userID;
        public Guid userGuid;

        protected void OnGUI()
        {
            userID = EditorGUILayout.TextField("User ID", userID);

            EditorGUILayout.TextField("User GUID", userGuid.ToString());

            if (GUILayout.Button("New GUID"))
                userGuid = Guid.NewGuid();

            if (GUILayout.Button("Create ID"))
            {
                DeveloperID.SetupID(userID, userGuid);
                Close();
            }
        }
    }
}