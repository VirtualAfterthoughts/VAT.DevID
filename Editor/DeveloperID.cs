using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

using UnityEngine;

namespace VAT.DevID
{
    [System.Serializable]
    public class DeveloperID
    {
        //
        // Actual Data
        //
        public string userID;
        public Guid userGuid;

        public DeveloperID() { }

        protected DeveloperID(string userID, Guid userGuid)
        {
            this.userID = userID;
            this.userGuid = userGuid;
        }

        //
        // Other stuff
        //
        public static DeveloperID CurrentID 
        { 
            get
            {
                if (_currentID == null)
                    TryLoadID();

                return _currentID;
            }
        }

        public static bool CreateIDLater
        {
            get
            {
                return File.Exists(CreateLaterPath);
            }
        }

        protected static DeveloperID _currentID = null;

        public static string PerUserFolderPath => $"{Application.persistentDataPath}/DevID";
        public static string PerProjectFolderPath => $"{Application.dataPath}/DevID".Replace("/Assets", "");

        protected static string IDPath => $"{PerUserFolderPath}/id.xml";
        protected static string CreateLaterPath => $"{PerUserFolderPath}/.createlater";

        protected static void ValidateFolders()
        {
            if (!Directory.Exists(PerUserFolderPath))
            {
                Directory.CreateDirectory(PerUserFolderPath);
                Debug.Log($"Created User DevID folder at '{PerUserFolderPath}'");
            }

            if (!Directory.Exists(PerProjectFolderPath))
            {
                Directory.CreateDirectory(PerProjectFolderPath);
                Debug.Log($"Created Project DevID folder at '{PerProjectFolderPath}'");
            }
        }

        protected static void TryLoadID()
        {
            ValidateFolders();

            if (File.Exists(IDPath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(DeveloperID));
                using (Stream stream = File.Open(IDPath, FileMode.Open))
                {
                    _currentID = (DeveloperID)serializer.Deserialize(stream);
                }
            }
        }

        public static void DeleteID()
        {
            File.Delete(IDPath);
            _currentID = null;
        }

        public static void SetupID(string userID, Guid userGuid)
        {
            ValidateFolders();

            XmlSerializer serializer = new XmlSerializer(typeof(DeveloperID));
            

            using (Stream stream = File.Open(IDPath, FileMode.Create))
            {
                serializer.Serialize(stream, new DeveloperID(userID, userGuid));
            }
        }

        public static void MarkCreateLater()
        {
            try
            {
                File.Create(CreateLaterPath)?.Close();
            }
            catch
            {

            }
        }
    }
}