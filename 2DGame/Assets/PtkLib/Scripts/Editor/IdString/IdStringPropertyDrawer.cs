using System;
using UnityEngine;
using UnityEditor;


namespace Ptk.IdStrings.Editor
{
	[CustomPropertyDrawer(typeof(IdString))]
	public class IdStringPropertyDrawer : PropertyDrawer
	{
		private static readonly GUIContent sTmp = new();

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label = EditorGUI.BeginProperty(position, label, property);
			position = EditorGUI.PrefixLabel(position, label);

			var prevIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			Color prevColor = GUI.color;

			var fullNameProperty = property.FindPropertyRelative("mFullName");
			var fullNameString = fullNameProperty.stringValue;
			var idString = IdString.Get( fullNameString );

			if( idString == IdString.None )
			{
				if( string.IsNullOrEmpty( fullNameString ) )
				{
					GUI.color = Color.gray;
					sTmp.text = $"None ({nameof(IdString)})";
					sTmp.tooltip = null;
				}
				else
				{
					sTmp.text = $"Missing ({nameof(IdString)}:{fullNameString})";
					sTmp.tooltip = null;
				}
			}
			else
			{
				//sTmp.text = idString.FullName;
				//sTmp.tooltip = idString.Description; 
				string strText = null;
				string strTooltip = null;

				var attrData = idString.AttrData;
				if( attrData != null )
				{
					strText = attrData.ElementName;
					if( !string.IsNullOrEmpty( attrData.ParentFullPath ) )
					{
						strText += $" ({attrData.ParentFullPath})";
					}
					strTooltip = attrData.Attribute?.Description; 
				}
				sTmp.text = strText;
				sTmp.tooltip = strTooltip;
			}

			if (EditorGUI.DropdownButton(position, sTmp, FocusType.Keyboard))
			{
				IdStringTreeView tagTreeView = new(new UnityEditor.IMGUI.Controls.TreeViewState(), fullNameProperty, static () =>
				{
				   EditorWindow.GetWindow<PopupWindow>().Close();
				});
				IdStringPopupWindowContent.ShowWindow( position, 320.0f, tagTreeView );

			}

			GUI.color = prevColor;
			EditorGUI.indentLevel = prevIndent;
			EditorGUI.EndProperty();
		}
	}



}