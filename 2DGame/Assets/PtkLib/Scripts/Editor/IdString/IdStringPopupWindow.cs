using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;


namespace Ptk.IdStrings.Editor
{

	public class IdStringPopupWindowContent : PopupWindowContent
	{

		private IdStringTreeView m_TreeView;
		private float m_Width;
		private float m_MaxHeight;

		public IdStringPopupWindowContent(float width, float maxHeight, IdStringTreeView tagTreeView)
		{
			m_Width = width;
			m_MaxHeight = maxHeight;
			m_TreeView = tagTreeView;
		}

		public override void OnGUI(Rect rect)
		{
			m_TreeView.OnGUI(rect);
		}

		public override Vector2 GetWindowSize()
		{
			return new Vector2(m_Width, m_MaxHeight);
		}

		public static void ShowWindow( Rect activatorRect, float maxHeight, IdStringTreeView treeView )
		{
			var self = new IdStringPopupWindowContent(activatorRect.width, maxHeight, treeView);
			PopupWindow.Show(activatorRect, self);
		}
	}

	public class IdStringTreeViewItem : TreeViewItem
	{
		private IdString mIdString;
		public IdString IdString => mIdString;

		public IdStringTreeViewItem( IdString idString, string elementName, int depth )
			: base( idString.Id, depth, elementName)
		{
			mIdString = idString;
		}
	}

	public class IdStringTreeView : TreeView
	{
		private static GUIContent sTmp = new();

		private SerializedProperty mProperty;
		private Action mActionSelectionChanged;

		private SearchField mSearchField = new();

		public IdStringTreeView(TreeViewState state, SerializedProperty property, Action onSelectionChanged = null) 
			: base(state)
		{
			showAlternatingRowBackgrounds = true;

			Reload();

			mActionSelectionChanged = onSelectionChanged;
			mProperty = property;
			mProperty.serializedObject.Update();

			var idString = IdString.Get(mProperty.stringValue);
			if (idString != IdString.None)
			{
				var item = FindItem(idString.Id, rootItem);
				if (item != null)
				{
					SetSelection(new int[] { item.id });
				}

				while (item != null)
				{
					SetExpanded(item.id, true);
					item = item.parent;
				}
			}
		}

		public float GetTotalHeight()
		{
			return totalHeight + EditorStyles.toolbar.fixedHeight;
		}

		protected override bool CanMultiSelect(TreeViewItem item)
		{
			return false;
		}

		protected override TreeViewItem BuildRoot()
		{
			TreeViewItem root = new(-2, -1, "<Root>");
			List<TreeViewItem> items = new();

			foreach( var attrData in IdStringManager.GetAllElements() )
			{
				if( attrData == null ){ continue; }
				items.Add(new IdStringTreeViewItem(attrData.IdString, attrData.ElementName, attrData.Hierarchy.Depth ));
			}

			SetupParentsAndChildrenFromDepths( root, items );
			return root;
		}

		protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
		{
			var idStringItem = item as IdStringTreeViewItem;
			if( idStringItem == null ){ return false; }
			var idString = idStringItem.IdString;
			var fullName = idString != null ? idString.FullName : null;
			if( fullName == null ){ return false; }
			return idStringItem.IdString.FullName.Contains( search, StringComparison.OrdinalIgnoreCase );
		}

		public override void OnGUI(Rect rect)
		{
			Rect toolbarRect = rect;
			toolbarRect.height = EditorStyles.toolbar.fixedHeight;
			
			GUILayout.BeginArea(rect);
			GUILayout.BeginHorizontal(EditorStyles.toolbar);

			
			if( GUILayout.Button("Expand All", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)) )
			{
				ExpandAll();
			}

			if(GUILayout.Button("Collapse All", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
			{
				CollapseAll();
			}

			//if( GUILayout.Button("Reset", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)) )
			//{
			//	mProperty.stringValue = null;
			//	mProperty.serializedObject.ApplyModifiedProperties();
			//}

			searchString = mSearchField.OnToolbarGUI(searchString);

			GUILayout.EndHorizontal();
			GUILayout.EndArea();
			rect.yMin += toolbarRect.height;

			base.OnGUI(rect);
		}

		protected override void RowGUI(RowGUIArgs args)
		{
			EditorGUI.BeginChangeCheck();

			float indent = GetContentIndent(args.item);
			Rect rect = args.rowRect;
			rect.xMin += indent - (hasSearch ? 14 : 0);

			var idStringItem = args.item as IdStringTreeViewItem;
			var idString = idStringItem != null ? idStringItem.IdString : IdString.None;
			var attrData = idString.AttrData;
			var idStringFullName = idString != IdString.None ? idString.FullName : null;

			sTmp.text = hasSearch && idString != IdString.None ? idStringFullName : args.label;
			sTmp.tooltip = attrData == null ? null : attrData.Description;
			if( GUI.Button( rect, sTmp, EditorStyles.label ) )
			{
				mProperty.stringValue = idStringFullName;
				mProperty.serializedObject.ApplyModifiedProperties();

				mActionSelectionChanged?.Invoke();
			}

			EditorGUI.EndChangeCheck();
		}

	}


}