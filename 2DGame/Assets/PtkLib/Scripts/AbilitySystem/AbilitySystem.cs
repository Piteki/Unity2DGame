using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ptk.AbilitySystems
{
	public class AbilitySystem : MonoBehaviour
	{
		public const int AbilityDefaultCapacity = 64;

		[SerializeField] private List< AbilityData > _Abilities = new( AbilityDefaultCapacity );

		/// <summary> アビリティリスト </summary>
		/// <remarks> ※外部からの更新厳禁 </remarks>
		public List< AbilityData > Abilities => _Abilities;


		protected void OnEnable()
		{
			foreach( var ability in _Abilities )
			{
				if( ability == null ){ continue; }
				ability.OnEnterSystem( this );
			}
		}

		protected void OnDisable()
		{
			foreach( var ability in _Abilities )
			{
				if( ability == null ){ continue; }
				ability.OnExitSystem();
			}
		}

		protected void Update()
		{
			foreach( var ability in _Abilities )
			{
				if( ability == null ){ continue; }
				// auto execute
				if( !ability.IsExecuting )
				{
					if( !ability.AutoExecute ){ continue; }
					ability.Execute();
					if( !ability.IsExecuting ){ continue; }
				}
				ability.Update();
			}
		}

		/// <summary>
		/// Ability 追加
		/// </summary>
		/// <param name="abilityData"></param>
		public void AddAbility( AbilityData abilityData )
		{
			if( abilityData == null ){ return; }
			if( _Abilities.Contains( abilityData ) ){ return; }
			_Abilities.Add( abilityData );
			if( enabled )
			{
				abilityData.OnEnterSystem( this );
			}
		}

		/// <summary>
		/// Ability 削除
		/// </summary>
		public void RemoveAbility( AbilityData abilityData )
		{
			if( abilityData == null ){ return; }
			if( !_Abilities.Contains( abilityData ) ){ return; }
			_Abilities.Remove( abilityData );
			abilityData.OnExitSystem();
		}

		/// <summary>
		/// Ability 取得
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetAbility< T >() where T : AbilityData
		{
			return _Abilities.Find( ability => ability is T ) as T;
		}

#if UNITY_EDITOR

		[CustomEditor(typeof(AbilitySystem), true)]
		public class AbilitySystemCustomEditor : Editor
		{

			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();

				var abilitySystem = target as AbilitySystem;
				if( abilitySystem == null ) { return; }

				EditorGUILayout.LabelField( "Abilities" );
				++EditorGUI.indentLevel;
				foreach( var ability in abilitySystem._Abilities )
				{
					if( ability == null ){ continue; }
					using( new EditorGUI.DisabledGroupScope( !ability.CanExecute ))
					{
						var style = ability.IsExecuting ? EditorStyles.boldLabel : EditorStyles.label;
						EditorGUILayout.LabelField( ability.AbilityName, style );
					}
				}
				--EditorGUI.indentLevel;
			}
		}
#endif // UNITY_EDITOR
		
	}

	public class AbilityData : ScriptableObject 
	{
		[SerializeField] private bool _AutoExecute = false;
		[SerializeField] private bool _RestartWhenExecuting = false;

		public AbilitySystem AbilitySystem { get; protected set; }
		public bool AutoExecute { get => _AutoExecute; set => _AutoExecute = value; }
		public bool RestartWhenExecuting { get => _RestartWhenExecuting; set => _RestartWhenExecuting = value; }
		public bool CanExecute => CheckCanExecute();
		public bool IsExecuting { get; protected set; }

#if UNITY_EDITOR
		public string AbilityName => this.GetType().Name;
#endif // UNITY_EDITOR

		public virtual void OnEnterSystem( AbilitySystem abilitySystem )
		{
			AbilitySystem = abilitySystem;
		}

		public virtual void OnExitSystem()
		{
			AbilitySystem = null;
		}

		protected virtual bool CheckCanExecute()
		{
			return true;
		}

		public bool Execute()
		{
			if( !RestartWhenExecuting
			 && IsExecuting
			){ return false; }

			if( !CanExecute ){ return false; }
			
			IsExecuting = true;
			OnExecuted();
			return true;
		}

		public void Update() 
		{
			if( !IsExecuting ){ return; }
			OnUpdate();
		}
		public void Finish() 
		{
			if( !IsExecuting ){ return; }
			IsExecuting = false;
			OnFinished();
		}


		protected virtual void OnExecuted(){}

		protected virtual void OnFinished(){}

		protected virtual void OnUpdate(){}
	}

}
