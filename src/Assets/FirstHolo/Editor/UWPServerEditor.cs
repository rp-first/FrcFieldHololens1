//    Copyright 2016 United States Government as represented by the
//    Administrator of the National Aeronautics and Space Administration.
//    All Rights Reserved.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//      http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.



using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Xml.Serialization;
using Xml2CSharp;


[CustomEditor(typeof(UWPServer))]
public class UWPServerEditor : Editor {

    SerializedProperty runAtStartup;
	SerializedProperty emulateServerConnection;
    Config _loadedConfig = null;

    void OnEnable()
    {
        runAtStartup = serializedObject.FindProperty("runAtStartup");
		emulateServerConnection = serializedObject.FindProperty("emulateServerConnection");

        _loadedConfig = null;
        
        TextAsset ta = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/Config/message_config.xml");
        if (ta)
        {
            XmlSerializer xs = new XmlSerializer(typeof(Config));
            _loadedConfig = (Config)xs.Deserialize(new System.IO.StringReader(ta.text));
        }
#if UNITY_EDITOR
        if (ta == null) throw new System.Exception("No config for UWPServer, Editor requires a config file.");
#endif
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(runAtStartup);
		EditorGUILayout.PropertyField(emulateServerConnection);
        EditorGUILayout.Space();
        if(_loadedConfig != null)
        {
            int i = 0;
            GUILayout.Label("Values defined in config (xml):");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Index", GUILayout.Width(64));
            GUILayout.Label("Type", GUILayout.Width(64));
            GUILayout.Label("Default", GUILayout.Width(64));
            GUILayout.Label("Name");
            EditorGUILayout.EndHorizontal();
            foreach (var item in _loadedConfig.Message.Value)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.IntField(i, GUILayout.Width(64));
                EditorGUILayout.TextField(item.Type, GUILayout.Width(64));
                EditorGUILayout.TextField(item.Default, GUILayout.Width(64));
                EditorGUILayout.TextField(item.Name);
                EditorGUILayout.EndHorizontal();
                ++i;
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
