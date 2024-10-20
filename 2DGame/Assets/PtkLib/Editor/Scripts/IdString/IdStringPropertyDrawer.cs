using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using Ptk.Editors;


namespace Ptk.IdStrings.Editor
{
	[CustomPropertyDrawer(typeof(IdString))]
	public class IdStringPropertyDrawer : PropertyDrawer
	{
		private static readonly GUIContent sTmp = new();

		/// <summary>
		/// 起動時の初期化
		/// </summary>
		[InitializeOnLoadMethod]
		private static void InitializeMenu()
		{
			// 右クリックメニュー追加
			EditorApplication.contextualPropertyMenu -= OnContextualPropertyMenu;
			EditorApplication.contextualPropertyMenu += OnContextualPropertyMenu;
		}

		/// <summary>
		/// 右クリックメニュー
		/// </summary>
		/// <param name="menu"></param>
		/// <param name="property"></param>
		private static void OnContextualPropertyMenu( GenericMenu menu, SerializedProperty property )
		{
			var fieldInfo = property.GetPropertyOrFieldMemberInfo() as FieldInfo;
			var type = fieldInfo?.FieldType;
			if( type != typeof( IdString ) ) { return; }
			var targetObject = property.serializedObject.targetObject;

			// 文字列コピー
			menu.AddItem( new GUIContent( "Copy Raw string" ), false, () => 
			{ 
				var value = (IdString)fieldInfo.GetValue( targetObject );
				EditorGUIUtility.systemCopyBuffer = value.FullName;
			});

			// 文字列ペースト
			menu.AddItem( new GUIContent( "Paste Raw string" ), false, () => 
			{ 
				var rawString = EditorGUIUtility.systemCopyBuffer;
				rawString = IdStringManager.GetSanitizedString( rawString );
				var newValue = IdStringManager.GetByNameOrCreateMissingReference( rawString );
				var value = (IdString)fieldInfo.GetValue( targetObject );
				if( value == newValue ){ return; }

				Undo.RecordObject( targetObject, $"Paste {property.displayName}" );
				
				fieldInfo.SetValue( targetObject, newValue );

				EditorUtility.SetDirty( targetObject );
			});
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label = EditorGUI.BeginProperty(position, label, property);
			position = EditorGUI.PrefixLabel(position, label);

			var prevIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			Color prevColor = GUI.color;

			var fullNameProperty = property.FindPropertyRelative("mFullName");
			var fullNameString = fullNameProperty.stringValue;
			IdString.TryGetByName( fullNameString, out var idString );

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
					GUI.color = Color.yellow;
					sTmp.text = $"Missing ({nameof(IdString)}:{fullNameString})";
					sTmp.tooltip = null;
				}
			}
			else
			{
				string strText = null;
				string strTooltip = null;

				var attrData = idString.AttrData;
				if( attrData != null )
				{
					strText = attrData.ElementName;
					if( !string.IsNullOrEmpty( attrData.ParentPath ) )
					{
						strText += $" ({attrData.ParentPath})";
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