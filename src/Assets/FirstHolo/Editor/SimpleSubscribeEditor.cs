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



using System;
using UnityEditor;
using FirstUtilities;
using NamedMessageValuesExtention;


[CustomEditor(typeof(SimpleSubscibeLabel))]
[CanEditMultipleObjects()]
public class SimpleSubscribeLabelEditor : Editor
{
	SerializedProperty subscibeIndex;
	SerializedProperty dataText;
	SerializedProperty nameText;

	void OnEnable()
	{
		subscibeIndex = serializedObject.FindProperty("subscibeIndex");
		dataText = serializedObject.FindProperty("dataText");
		nameText = serializedObject.FindProperty("nameText");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		EditorGUILayout.PropertyField(nameText);
		EditorGUILayout.PropertyField(dataText);
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.PrefixLabel ("Subscibed To:");
		MessageEnum eValue = (MessageEnum)EditorGUILayout.EnumPopup ((MessageEnum)subscibeIndex.intValue);
		if(subscibeIndex.intValue != (int)eValue) {
			subscibeIndex.intValue = (int)eValue;
		}
		EditorGUILayout.EndHorizontal ();

		serializedObject.ApplyModifiedProperties();
	}
}


