#define _IDSTRING_TEST_ENABLED

#if _IDSTRING_TEST_ENABLED

using NUnit.Framework;
using Ptk.IdStrings;

[assembly:IdStringDefine( "IdStringTest.assembly.A1.B1.C1" )]
[assembly:IdStringDefine( "IdStringTest.assembly.A1.B1.C2" )]
[assembly:IdStringDefine( "IdStringTest.assembly.A1.B2" )]
[assembly:IdStringDefine( "IdStringTest.assembly.A1.B2.C1" )]
[assembly:IdStringDefine( "IdStringTest.assembly.A2.B1" )]
[assembly:IdStringDefine( "IdStringTest.assembly.A3" )]

namespace Ptk.IdStrings
{
	[IdStringDefineMember]
	public static class IdStringTestClass
	{
		[IdStringDefineMember]
		public static IdString A1 { get; private set; }

		[IdStringDefineMember]
		public static IdString A2 { get; private set; }

		[IdStringDefineMember]
		public static class A3 
		{
			[IdStringDefineMember]
			public static IdString B1 { get; private set; }
			[IdStringDefineMember]
			public static IdString B2 { get; private set; }

			[IdStringDefineMember]
			public static class B3 
			{
				[IdStringDefineMember]
				public static IdString C1 { get; private set; }

				[IdStringDefineMember]
				public static IdString C2 { get; private set; }
			}

		}

		public static IdString A4 { get; private set; }

		public static class A5 
		{
			[IdStringDefineMember]
			public static IdString B1 { get; private set; }

			[IdStringDefineMember]
			public static class B2
			{
				[IdStringDefineMember]
				public static IdString C1 { get; private set; }

				[IdStringDefineMember]
				public static IdString C2 { get; private set; }
			}

		}
	}


	internal class IdStringsEditorTests
	{
				
		[Test] public void CheckAssemblyDefine()
		{
			Assert.That( IdString.Get( "IdStringTest.assembly.A1.B1.C1" ) != IdString.None );
		}
		[Test] public void CheckMemberDefine()
		{
			Assert.That( IdString.Get( "IdStringTestClass.A3.B1" ) == IdStringTestClass.A3.B1 );
			Assert.That( IdString.Get( "IdStringTestClass.A3.B" ) != IdStringTestClass.A3.B1 );
		}
		[Test] public void CheckContainer()
		{
			var container = new IdStringContainer();
			container.Add( IdStringTestClass.A3.B2 );
			container.Add( IdStringTestClass.A2 );
			container.Add( IdStringTestClass.A3.B1 );

			Assert.IsTrue( container.Has( IdStringTestClass.A3.B1 ) );
			Assert.IsTrue( container.HasExact( IdStringTestClass.A3.B1 ) );
			Assert.IsTrue( container.Has( IdString.Get(typeof(IdStringTestClass.A3)) ) );
			Assert.IsFalse( container.HasExact( IdString.Get(typeof(IdStringTestClass.A3)) ) );

			container.Remove( IdStringTestClass.A3.B1 );

			Assert.IsFalse( container.Has( IdStringTestClass.A3.B1 ) );
			Assert.IsFalse( container.HasExact( IdStringTestClass.A3.B1 ) );
			Assert.IsTrue( container.Has( IdString.Get(typeof(IdStringTestClass.A3)) ) );

			container.Remove( IdStringTestClass.A3.B2 );
			Assert.IsFalse( container.HasExact( IdStringTestClass.A3.B1 ) );
			Assert.IsFalse( container.Has( IdString.Get(typeof(IdStringTestClass.A3)) ) );


			var containerB = new IdStringContainer(
			new IdString[]{
				IdStringTestClass.A3.B2,
				IdStringTestClass.A3.B1,
			});

			container.Add( IdStringTestClass.A3.B2 );
			container.Add( IdStringTestClass.A3.B1 );

			Assert.IsTrue( container.HasAll( containerB ) );
			Assert.IsTrue( container.HasAllExact( containerB ) );

			containerB.Add( IdString.Get(typeof(IdStringTestClass.A3)) );

			Assert.IsTrue( container.HasAll( containerB ) );
			Assert.IsFalse( container.HasAllExact( containerB ) );


			container.Remove( IdStringTestClass.A3.B1 );
			Assert.IsFalse( container.HasAll( containerB ) );
			Assert.IsFalse( container.HasAllExact( containerB ) );

			Assert.IsTrue( container.HasAny( containerB ) );
			Assert.IsTrue( container.HasAnyExact( containerB ) );

			var idStringDeleted = IdString.None;
			void OnElementChanged( IdString elem, bool isAdded )
			{
				 if( isAdded ){ return; }
				idStringDeleted = elem;
			}
			containerB.EventElementChanged += OnElementChanged;

			containerB.Remove( IdStringTestClass.A3.B2 );

			Assert.IsTrue( idStringDeleted == IdStringTestClass.A3.B2 );

			Assert.IsTrue( container.HasAny( containerB ) );
			Assert.IsFalse( container.HasAnyExact( containerB ) );

			Assert.IsTrue( container.FirstOrDefault() == container[0] );
			Assert.IsTrue( container.LastOrDefault() == container[container.Count-1] );
			Assert.IsFalse( container.ElementAtOrDefault(2) == container[1] );

			// TOD
			container.AddLeafElement( IdStringTestClass.A3.B1 );
			Assert.IsTrue( container.HasExact( IdStringTestClass.A3.B1 ) );
			Assert.IsFalse( container.HasExact( IdString.Get(typeof(IdStringTestClass.A3)) ) );
			container.Add( IdString.Get(typeof(IdStringTestClass.A3)) );
			container.AddLeafElement( IdString.Get(typeof(IdStringTestClass.A3.B3)) );
			Assert.IsFalse( container.HasExact( IdString.Get(typeof(IdStringTestClass.A3)) ) );
			Assert.IsTrue( container.Has( IdString.Get(typeof(IdStringTestClass.A3)) ) );
			Assert.IsTrue( container.HasExact( IdString.Get(typeof(IdStringTestClass.A3.B3)) ) );
			container.AddLeafElement( IdStringTestClass.A3.B3.C1 );
			container.AddLeafElement( IdStringTestClass.A3.B3.C2 );
			Assert.IsTrue( container.Has( IdString.Get(typeof(IdStringTestClass.A3)) ) );
			Assert.IsTrue( container.Has( IdString.Get(typeof(IdStringTestClass.A3.B3)) ) );
			Assert.IsFalse( container.HasExact( IdString.Get(typeof(IdStringTestClass.A3)) ) );
			Assert.IsFalse( container.HasExact( IdString.Get(typeof(IdStringTestClass.A3.B3)) ) );
			Assert.IsTrue( container.HasExact( IdStringTestClass.A3.B3.C1 ) );
			Assert.IsTrue( container.HasExact( IdStringTestClass.A3.B3.C2 ) );
		}
	}
		
}


#endif //_IDSTRING_TEST_ENABLED
