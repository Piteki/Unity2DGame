using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ptk
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
				ability.OnEnable( this );
			}
		}

		protected void OnDisable()
		{
			foreach( var ability in _Abilities )
			{
				if( ability == null ){ continue; }
				ability.OnDisable();
			}
		}

		protected void Update()
		{
			foreach( var ability in _Abilities )
			{
				if( ability == null ){ continue; }
				if( !ability.IsExecuting )
				{
					if( ability.CheckExecute() ){ continue; }
					ability.Execute();
				}
				ability.Update();
			}
		}

		public void AddAbility( AbilityData abilityData )
		{
			if( abilityData == null ){ return; }
			if( _Abilities.Contains( abilityData ) ){ return; }
			_Abilities.Add( abilityData );
			if( enabled )
			{
				abilityData.OnEnable( this );
			}
		}
		public void RemoveAbility( AbilityData abilityData )
		{
			if( abilityData == null ){ return; }
			if( !_Abilities.Contains( abilityData ) ){ return; }
			_Abilities.Remove( abilityData );
			abilityData.OnDisable();
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

				EditorGUILayout.LabelField( "Own Abilities" );

				foreach( var ability in abilitySystem._Abilities )
				{
					if( ability == null ){ continue; }
					EditorGUILayout.LabelField( ability.AbilityName );
				}
				
				EditorGUILayout.LabelField( "Executing Abilities" );
				foreach( var ability in abilitySystem._Abilities )
				{
					if( ability == null ){ continue; }
					EditorGUILayout.LabelField( ability.AbilityName );
				}
			}
		}
#endif // UNITY_EDITOR
		
	}

	public class AbilityData : ScriptableObject 
	{
		[SerializeField] private string _AbilityName;

		public string AbilityName => _AbilityName;

		public AbilitySystem AbilitySystem { get; protected set; }
		public bool IsExecuting { get; protected set; }

		public virtual void OnEnable( AbilitySystem abilitySystem )
		{
			AbilitySystem = abilitySystem;
		}

		public virtual void OnDisable()
		{
		}

		public virtual bool CheckExecute()
		{
			return false;
		}

		public virtual void Execute()
		{
			IsExecuting = true;
		}

		public virtual void Finish() 
		{
			IsExecuting = false;
		}

		public virtual void Update()
		{

		}
	}


	[CreateAssetMenu(fileName = "New JumpAbility.asset", menuName = "2DGame/Ability/Jump")]
	public class AbilityData_Jump : AbilityData 
	{
		public override bool CheckExecute()
		{
			return false;
		}


		public override void Execute()
		{
			base.Execute();
			
		}

	}
}
