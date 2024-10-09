using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



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

		private List< AbilityBase > _AbilityInstances = new( AbilityDefaultCapacity );

		protected void OnEnable()
		{
			_AbilityInstances.Clear();
			foreach( var abilityData in _Abilities )
			{
				AddAbilityInstance( abilityData );
			}
		}

		protected void OnDisable()
		{
			foreach( var ability in _AbilityInstances )
			{
				if( ability == null ){ continue; }
				ability.OnExitSystem();
			}
			_AbilityInstances.Clear();
		}

		protected void Update()
		{
			foreach( var ability in _AbilityInstances )
			{
				if( ability == null ){ continue; }
				// auto execute
				if( !ability.IsExecuting )
				{
					var data = ability.GetBaseAbilityData();
					var autoExecute = data != null ? data.AutoExecute : false;
					if( !autoExecute ){ continue; }
					ability.Execute();
					if( !ability.IsExecuting ){ continue; }
				}
				ability.Update();
			}
		}

		private void AddAbilityInstance( AbilityData data )
		{
			if( data == null ){ return; }
			var abilityInstance = _AbilityInstances.Find( ability => ability.GetBaseAbilityData() == data );
			if( abilityInstance == null )
			{
				abilityInstance = data.CreateAbilityBaseInstance();
				_AbilityInstances.Add( abilityInstance );
			}
			if( enabled )
			{
				abilityInstance.OnEnterSystem( this );
			}
		}

		private void RemoveAbilityInstance( AbilityBase ability )
		{
			if( ability == null ){ return; }
			if( !_AbilityInstances.Remove( ability ) ){ return; }
			ability.OnExitSystem();
		}

		/// <summary>
		/// Ability 追加
		/// </summary>
		/// <param name="abilityData"></param>
		public bool AddAbility( AbilityData abilityData )
		{
			if( abilityData == null ){ return false; }
			if( _Abilities.Contains( abilityData ) ){ return false; }
			_Abilities.Add( abilityData );
			if( enabled )
			{
				AddAbilityInstance( abilityData );
			}
			return true;
		}

		/// <summary>
		/// Ability 削除
		/// </summary>
		public bool RemoveAbility( AbilityData abilityData )
		{
			if( abilityData == null ){ return false; }
			var abilityInstance = _AbilityInstances.Find( ability => ability.GetBaseAbilityData() == abilityData );
			if( abilityInstance != null )
			{
				RemoveAbilityInstance( abilityInstance );
			}
			return _Abilities.Remove( abilityData );
		}

		/// <summary>
		/// Ability 取得
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetAbility< T >() where T : AbilityBase
		{
			return _AbilityInstances.Find( ability => ability is T ) as T;
		}

		/// <summary>
		/// アビリティ 所持チェック
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public bool HasAbility< T >() where T : AbilityBase
		{
			return GetAbility< T >() != null;
		}

		/// <summary>
		/// アビリティ 発動中チェック
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public bool IsExecuting< T >() where T : AbilityBase
		{
			var ability = GetAbility< T >();
			return ability != null ? ability.IsExecuting : false;
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

				EditorGUILayout.LabelField( "Ability Instances" );
				++EditorGUI.indentLevel;
				foreach( var ability in abilitySystem._AbilityInstances )
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

	/// <summary>
	/// Ability Data
	/// </summary>
	public abstract class AbilityData : ScriptableObject 
	{
		[SerializeField] private bool _AutoExecute = false;
		[SerializeField] private bool _RestartWhenExecuting = false;

		public bool AutoExecute { get => _AutoExecute; set => _AutoExecute = value; }
		public bool RestartWhenExecuting { get => _RestartWhenExecuting; set => _RestartWhenExecuting = value; }

		public abstract AbilityBase CreateAbilityBaseInstance();


	}


	/// <summary>
	/// Ability Data
	/// </summary>
	public class AbilityData< TAbilityClass, TDataClass > : AbilityData 
		where TDataClass : AbilityData< TAbilityClass, TDataClass >
		where TAbilityClass : AbilityBase< TAbilityClass, TDataClass >, new()
	{
		public override AbilityBase CreateAbilityBaseInstance(){ return CreateAbilityInstance(); }

		public TAbilityClass CreateAbilityInstance()
		{
			var instance = new TAbilityClass();
			instance.SetData( this as TDataClass );
			return instance;
		}

	}

	/// <summary>
	/// Ability 基底クラス
	/// </summary>
	public abstract class AbilityBase
	{
		public AbilitySystem AbilitySystem { get; protected set; }
		public bool CanExecute => CheckCanExecute();
		public bool IsExecuting { get; protected set; }

#if UNITY_EDITOR
		public string AbilityName => this.GetType().Name;
#endif // UNITY_EDITOR

		public abstract AbilityData GetBaseAbilityData();

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
			var data = GetBaseAbilityData();
			bool restart = data == null ? false : data.RestartWhenExecuting;
			if( !restart
			 && IsExecuting
			){ return false; }
			return true;
		}

		public bool Execute()
		{
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

	/// <summary>
	/// Ability 基底クラス
	/// </summary>
	public class AbilityBase< TAbilityClass, TDataClass > : AbilityBase
		where TDataClass : AbilityData< TAbilityClass, TDataClass >
		where TAbilityClass : AbilityBase< TAbilityClass, TDataClass >, new()
	{
		public TDataClass Data { get; private set; }

		public override AbilityData GetBaseAbilityData(){ return Data; }

		public void SetData( TDataClass data )
		{
			Data = data;
		}

	}

}
