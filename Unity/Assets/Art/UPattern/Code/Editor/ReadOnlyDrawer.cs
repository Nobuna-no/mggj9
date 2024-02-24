using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect rPos, SerializedProperty SProp, GUIContent Lable)
	{
		// This is the read only value
		string sValue;
		
		switch (SProp.propertyType)
		{
		case SerializedPropertyType.Integer:
			sValue = SProp.intValue.ToString();
			break;
		case SerializedPropertyType.Float:
			sValue = SProp.floatValue.ToString("0.00000");
			break;
		case SerializedPropertyType.Boolean:
			sValue = SProp.boolValue.ToString();
			break;
		case SerializedPropertyType.String:
			sValue = SProp.stringValue;
			break;
		default:
			sValue = "(Value is not supported)";
			break;
		}

		EditorGUI.LabelField(rPos,Lable.text, sValue);
	}
}