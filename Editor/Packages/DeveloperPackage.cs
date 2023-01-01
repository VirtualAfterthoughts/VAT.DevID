using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

using UnityEngine;

namespace VAT.DevID.Packages 
{
    [System.Serializable]
    public class DeveloperPackage
    {
        public enum SourceLocation 
        {
            Local, Git
        }

        [System.Serializable]
        public class PerUserLocation
        {
            public Guid user;
            public string path;
            public SourceLocation sourceLocation;

            [NonSerialized]
            [XmlIgnore]
            public bool editorFoldout = false;
        }

        public string name;

        public string identifier;
        public string defaultPath;
        public SourceLocation defaultSourceLocation;
        public List<PerUserLocation> perUserLocations = new List<PerUserLocation>();

        [NonSerialized]
        [XmlIgnore]
        public bool editorFoldout = false;

        public static string PackagesFolderPath => $"{DeveloperID.PerProjectFolderPath}/Packages";

        public static void ValidateFolder()
        {
            if (!Directory.Exists(PackagesFolderPath))
            {
                Directory.CreateDirectory(PackagesFolderPath);
                Debug.Log($"Created Packages folder at '{PackagesFolderPath}'");
            }
        }
    }
}
