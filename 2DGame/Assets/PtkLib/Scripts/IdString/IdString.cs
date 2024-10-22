using System;
using UnityEngine;


namespace Ptk.IdStrings
{
	/// <summary>
	/// IdString
	/// </summary>
	/// <remarks>
	/// string �萔�� int �^�萔�̃R�X�g�ň��� struct�B
	/// IdString Attribute �ŕ�������`���鎖�� Domain Load ��ɂ͂��̕������ int �^�� ID �Ƃ��Ĉ��������\�ɂȂ�B
	/// Serialize ���� string �ŊO���ۑ����s���ADeserialize ���͕����񌟍��� ID �ɕϊ������B
	/// �܂��l�X�g���� static field �Ƃ��đ������`���鎖�ŊK�w�\�����\�B
	/// ������萔��񋓌^�̑�ւȂǕ��L���p�r�Ŏg�p�\�B
	/// </remarks>
	[Serializable]
	public struct IdString : IEquatable< IdString >, ISerializationCallbackReceiver
	{
		public static readonly IdString None = new();

		[SerializeField] private string mFullName;
		private int mId;

		/// <summary>
		/// Full Name 
		/// </summary>
		/// <remarks>
		/// Serialize �Ώۂ̕�����B
		/// </remarks>
		public string FullName => mFullName;

		/// <summary>
		/// Id
		/// </summary>
		/// <remarks>
		/// IdStringManager �����̓o�^ Id�B
		/// ������r�ȂǂɎg�p�����B
		/// �l�� Domain Reload �̂��тɕύX����邽�߂����ۑ����Ďg�p���Ȃ����ƁB
		/// </remarks>
		internal int Id => mId;

		/// <summary>
		/// attribute data
		/// </summary>
		internal IdStringAttrData AttrData => IdStringManager.GetAttrData( this );

		/// <summary>
		/// �v�f��
		/// </summary>
		/// <remarks>
		/// FullName ���� ParentPath ��������������B
		/// </remarks>
		public string ElementName
		{
			get{
				var attrData = AttrData;
				if( attrData == null ){ return null; }
				return attrData.ElementName;
			}
		}

		/// <summary>
		/// �K�w���x��
		/// </summary>
		public int HierarchyLevel
		{
			get{
				var attrData = AttrData;
				if( attrData == null ){ return 0; }
				return attrData.Hierarchy.Depth;
			}
		}

		/// <summary>
		/// ����
		/// </summary>
		public string Description
		{
			get{
				var attrData = AttrData;
				if( attrData == null ){ return null; }
				return attrData.Attribute?.Description;
			}
		}

		/// <summary>
		/// �L���l���B
		/// </summary>
		/// <remarks>
		/// IdString.None ���邢�� Missing �̏ꍇ�� false
		/// </remarks>
		public bool IsValid => this != None;


		/// <summary>
		/// �w��v�f�̎q���𔻒�
		/// </summary>
		/// <returns> �e�� parent �̏ꍇ true </returns>
		public bool IsChildOf( in IdString parent )
		{
			return IdStringManager.IsChildOf( this, parent );
		}
		
		/// <summary>
		/// �w��v�f�̎q�����𔻒�
		/// </summary>
		/// <returns> �c��� ancestor ���܂܂��ꍇ true </returns>
		public bool IsDescendantOf( in IdString ancestor )
		{
			return IdStringManager.IsDescendantOf( this, ancestor );
		}

		/// <summary>
		/// �����񂩂� IdString ���擾
		/// </summary>
		/// <param name="stringValue"> FullName ������ </param>
		/// <returns> �o�^�� IdString�B�Ȃ��ꍇ�� IdString.None ��Ԃ� Warning Log ���o�͂���B </returns>
		static public IdString Get( string stringValue ) => IdStringManager.GetByName( stringValue );

		/// <summary>
		/// �����񂩂� IdString ���擾
		/// </summary>
		/// <param name="stringValue"> FullName ������ </param>
		/// <param name="result"> �o�^�� IdString�B�Ȃ��ꍇ�� IdString.None�B </param>
		/// <returns> �o�^�� IdString �����������ꍇ true�B </returns>
		static public bool TryGetByName( string stringValue, out IdString result ) => IdStringManager.TryGetByName( stringValue, out result );
		
		/// <summary>
		/// Type ���� IdString ���擾
		/// </summary>
		/// <remarks>
		/// IdStringDefineMember Attribute ��t�^���� Type �Œ�`���� IdString ���擾����B
		/// </remarks>
		/// <typeparam name="T"> IdStringDefineMember ��t�^����Type </typeparam>
		/// <returns> �o�^�� IdString�B�Ȃ��ꍇ�� IdString.None ��Ԃ� Warning Log ���o�͂���B </returns>
		static public IdString Get< T >() => IdStringManager.GetByType< T >();
		
		/// <summary>
		/// Type ���� IdString ���擾
		/// </summary>
		/// <remarks>
		/// IdStringDefineMember Attribute ��t�^���� Type �Œ�`���� IdString ���擾����B
		/// </remarks>
		/// <param name="type"> IdStringDefineMember ��t�^����Type </param>
		/// <returns> �o�^�� IdString�B�Ȃ��ꍇ�� IdString.None ��Ԃ� Warning Log ���o�͂���B </returns>
		static public IdString Get( Type type ) => IdStringManager.GetByType( type );

		/// <summary>
		/// Type ���� IdString ���擾
		/// </summary>
		/// <remarks>
		/// IdStringDefineMember Attribute ��t�^���� Type �Œ�`���� IdString ���擾����B
		/// </remarks>
		/// <typeparam name="T"> IdStringDefineMember ��t�^����Type </typeparam>
		/// <param name="result"> �o�^�� IdString�B�Ȃ��ꍇ�� IdString.None�B </param>
		/// <returns> �o�^�� IdString �����������ꍇ true�B </returns>
		static public bool TryGetByType< T >( out IdString result ) => IdStringManager.TryGetByType< T >( out result );

		/// <summary>
		/// Type ���� IdString ���擾
		/// </summary>
		/// <remarks>
		/// IdStringDefineMember Attribute ��t�^���� Type �Œ�`���� IdString ���擾����B
		/// </remarks>
		/// <param name="type"> IdStringDefineMember ��t�^����Type </param>
		/// <param name="result"> �o�^�� IdString�B�Ȃ��ꍇ�� IdString.None�B </param>
		/// <returns> �o�^�� IdString �����������ꍇ true�B </returns>
		static public bool TryGetByType( Type type, out IdString result ) => IdStringManager.TryGetByType( type, out result );


		internal IdString( string fullName, int id )
		{
			mFullName = fullName;
			mId = id;
		}

		public override readonly string ToString()
		{
			return mFullName;
		}

		public override readonly int GetHashCode()
		{
			return mId;
		}

		public readonly bool Equals( IdString other )
		{
			return mId == other.mId;
		}

		public override readonly bool Equals( object obj )
		{
			if( obj is IdString hashTag )
			{
				return mId == hashTag.mId;
			}

			if( obj is string str )
			{
				return mFullName.Equals( str );
			}

			return false;
		}

		static public implicit operator IdString( string str )
		{
			return IdStringManager.GetByName( str );
		}

		static public bool operator ==( in IdString self, in IdString other )
		{
			return self.Equals( other );
		}
		static public bool operator !=( in IdString self, in IdString other )
		{
			return !self.Equals( other );
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			var idString = IdStringManager.GetByName( mFullName );
			if( idString != IdString.None )
			{
				this = idString;
			}
			else
			{
				// missing..
				mId = 0;
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

	}

}
