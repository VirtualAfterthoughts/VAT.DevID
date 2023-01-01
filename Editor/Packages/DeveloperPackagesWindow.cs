using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

using UnityEngine;

using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace VAT.DevID.Packages
{
    public class DeveloperPackagesWindow : EditorWindow
    {
        [MenuItem("VAT/DevID/Packages")]
        protected static void OpenWindow()
        {
            DeveloperPackagesWindow window = GetWindow<DeveloperPackagesWindow>();
            
            window.titleContent = new GUIContent("DevID Packages", "Manage DevID specfic packages!");

            window.Show();
        }

        [System.Serializable]
        public class DummyManifest
        {
            public Dictionary<string, string> dependencies = new Dictionary<string, string>();
        }

        protected bool scanForPackages = true;
        protected bool showPackageList = false;

        protected bool hadWindow = false;

        public List<DeveloperPackage> packages = new List<DeveloperPackage>();

        protected Queue<string> queuedAdds = new Queue<string>();
        protected AddRequest currentRequest;
        protected Vector2 scroll;

        protected void IndentBandaid()
        {
            GUILayout.Space(16.5F * EditorGUI.indentLevel);
        }

        protected void SwitchPackages(bool forceUseDefaults = false)
        {
            foreach (DeveloperPackage package in packages)
                SwitchPackage(package, forceUseDefaults, false);

            EditorApplication.update += CheckNext;
        }

        protected void SwitchPackage(DeveloperPackage package, bool forceUseDefaults = false, bool single = true) 
        {
            string path = package.defaultPath;
            DeveloperPackage.SourceLocation source = package.defaultSourceLocation;

            if (!forceUseDefaults)
            {
                foreach (DeveloperPackage.PerUserLocation location in package.perUserLocations)
                {
                    if (location.user == DeveloperID.CurrentID.userGuid)
                    {
                        path = location.path;
                        source = location.sourceLocation;
                    }
                }
            }

            Debug.Log($"Switching package '{package.name}' install to '{path}'");

            string realPath = "";
            switch (source)
            {
                case DeveloperPackage.SourceLocation.Git:
                    realPath = path;
                    break;

                case DeveloperPackage.SourceLocation.Local:
                    realPath = $"file:{path}";
                    break;
            }

            queuedAdds.Enqueue(realPath);

            if (single)
                EditorApplication.update += CheckNext;
        }

        protected void CheckNext()
        {
            if (queuedAdds.Count == 0)
                EditorApplication.update -= CheckNext;
            else 
            {
                bool canRequest = currentRequest == null;

                if (!canRequest)
                    canRequest = currentRequest.IsCompleted;

                if (canRequest)
                    currentRequest = Client.Add(queuedAdds.Dequeue());
            }
        }

        protected void OnGUI()
        {
            if (scanForPackages)
            {   
                DeveloperPackage.ValidateFolder();

                string path = DeveloperPackage.PackagesFolderPath;

                string[] packageXmlFiles = Directory.GetFiles(path, "*.xml");

                XmlSerializer serializer = new XmlSerializer(typeof(DeveloperPackage));

                packages.Clear();
                foreach (string packageXmlFile in packageXmlFiles) 
                {
                    using (Stream stream = File.Open(packageXmlFile, FileMode.Open))
                    {
                        DeveloperPackage package = (DeveloperPackage)serializer.Deserialize(stream);
                        packages.Add(package);
                    }
                }

                scanForPackages = false;
            }

            if (GUILayout.Button("Reload List"))
                scanForPackages = true;

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Switch Packages"))
                if (EditorUtility.DisplayDialog("Confirm", "Are you sure you want to switch packages?", "Yes", "No"))
                    SwitchPackages();

            if (GUILayout.Button("Switch Packages (Force Defaults)"))
                if (EditorUtility.DisplayDialog("Confirm", "Are you sure you want to switch packages?", "Yes", "No")) 
                    SwitchPackages(true);

            GUILayout.EndHorizontal();

            if ((showPackageList = EditorGUILayout.Foldout(showPackageList, "Packages")))
            {
                scroll = EditorGUILayout.BeginScrollView(scroll);

                using (new EditorGUI.IndentLevelScope(1))
                {
                    if (GUILayout.Button("Add New Package"))
                        packages.Add(new DeveloperPackage() { name = "New Package" });

                    GUILayout.Space(10F);

                    DeveloperPackage deletePackage = null;
                    foreach (DeveloperPackage package in packages)
                    {
                        GUILayout.BeginHorizontal();

                        IndentBandaid();
                        if (GUILayout.Button("-", GUILayout.Width(32)))
                            deletePackage = package;

                        if ((package.editorFoldout = EditorGUILayout.Foldout(package.editorFoldout, package.name)))
                        {
                            using (new EditorGUI.IndentLevelScope(1))
                            {
                                GUILayout.EndHorizontal();

                                if (deletePackage != null)
                                    break;

                                GUILayout.BeginHorizontal();

                                IndentBandaid();

                                if (GUILayout.Button("Switch"))
                                    SwitchPackage(package);

                                if (GUILayout.Button("Switch (Force Defaults)"))
                                    SwitchPackage(package, true);

                                GUILayout.EndHorizontal();

                                using (new GUILayout.HorizontalScope())
                                {
                                    IndentBandaid();
                                    package.name = EditorGUILayout.TextField("Name:", package.name);
                                }

                                using (new GUILayout.HorizontalScope())
                                {
                                    IndentBandaid();
                                    package.identifier = EditorGUILayout.TextField("ID:", package.identifier);
                                }

                                using (new GUILayout.HorizontalScope())
                                {
                                    IndentBandaid();
                                    package.defaultPath = EditorGUILayout.TextField("Path:", package.defaultPath);
                                }

                                using (new GUILayout.HorizontalScope())
                                {
                                    IndentBandaid();
                                    package.defaultSourceLocation = (DeveloperPackage.SourceLocation)EditorGUILayout.EnumPopup("Source:", package.defaultSourceLocation);
                                }

                                GUILayout.Space(8);

                                using (new GUILayout.HorizontalScope())
                                {
                                    IndentBandaid();
                                    EditorGUILayout.LabelField("Per-User Data");
                                }

                                using (new EditorGUI.IndentLevelScope(1))
                                {
                                    GUILayout.BeginHorizontal();

                                    IndentBandaid();
                                    if (GUILayout.Button("+ (You)"))
                                    {
                                        bool cantAdd = false;
                                        foreach (DeveloperPackage.PerUserLocation location in package.perUserLocations)
                                            if (location.user == DeveloperID.CurrentID.userGuid)
                                                cantAdd = true;

                                        if (!cantAdd)
                                            package.perUserLocations.Add(new DeveloperPackage.PerUserLocation() { user = DeveloperID.CurrentID.userGuid });
                                    }

                                    GUILayout.EndHorizontal();

                                    GUILayout.Space(2F);

                                    DeveloperPackage.PerUserLocation deleteLocation = null;
                                    foreach (DeveloperPackage.PerUserLocation location in package.perUserLocations)
                                    {

                                        GUILayout.BeginHorizontal();

                                        IndentBandaid();
                                        if (GUILayout.Button("-", GUILayout.Width(32)))
                                            deleteLocation = location;

                                        if ((location.editorFoldout = EditorGUILayout.Foldout(location.editorFoldout, location.user.ToString())))
                                        {
                                            GUILayout.EndHorizontal();

                                            using (new GUILayout.HorizontalScope())
                                            {
                                                IndentBandaid();
                                                EditorGUILayout.TextField("User:", location.user.ToString());
                                            }

                                            using (new GUILayout.HorizontalScope())
                                            {
                                                IndentBandaid();
                                                location.path = EditorGUILayout.TextField("Path:", location.path);
                                            }

                                            using (new GUILayout.HorizontalScope())
                                            {
                                                IndentBandaid();
                                                location.sourceLocation = (DeveloperPackage.SourceLocation)EditorGUILayout.EnumPopup("Source:", location.sourceLocation);
                                            }
                                        }
                                        else
                                        {
                                            GUILayout.EndHorizontal();
                                        }

                                        if (deleteLocation != null)
                                            break;
                                    }

                                    if (deleteLocation != null)
                                        package.perUserLocations.Remove(deleteLocation);
                                }
                            }
                        }
                        else
                        {
                            GUILayout.EndHorizontal();
                        }

                        GUILayout.Space(2F);

                        if (deletePackage != null)
                            break;
                    }

                    if (deletePackage != null)
                        packages.Remove(deletePackage);

                    EditorGUILayout.EndScrollView();

                    GUILayout.Space(10F);
                }
            }

            if (GUILayout.Button("Write Changes"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(DeveloperPackage));

                string[] packageXmlFiles = Directory.GetFiles(DeveloperPackage.PackagesFolderPath, "*.xml");

                foreach (string packageXmlFile in packageXmlFiles)
                    File.Delete(packageXmlFile);

                foreach (DeveloperPackage package in packages)
                {
                    string path = $"{DeveloperPackage.PackagesFolderPath}/{package.name}.xml";

                    using (Stream stream = File.Open(path, FileMode.Create))
                    {
                        serializer.Serialize(stream, package);
                    }
                }
            }
        }
    }
}