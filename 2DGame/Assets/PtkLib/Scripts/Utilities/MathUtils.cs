
using System;
using UnityEngine;

namespace Ptk
{
	static public class MathUtil
	{


		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// 除算
		/// </summary>
		static public float SafeDivide( this float num, float denom, float valueWhenError = default )
		{
			return IsZero( denom ) ? valueWhenError : num / denom;
		}

		/// <summary>
		/// 剰余
		/// </summary>
		static public float Mod(this float a, float b)
		{
			return a - Mathf.Floor(a / b) * b;
		}



		/// <summary>
		/// Zero チェック
		/// </summary>
		static public bool IsZero(this float value)
		{
			return Mathf.Abs(value) <= Mathf.Epsilon;
		}

		/// <summary>
		/// 比較
		/// </summary>
		static public bool IsAlmostEqual(this float value, float b)
		{
			return IsAlmostEqual(value, b, Mathf.Epsilon);
		}
		/// <summary>
		/// 比較
		/// </summary>
		static public bool IsAlmostEqual(this float value, float b, float precision)
		{
			return Mathf.Abs(value - b) <= precision;
		}

		/// <summary>
		/// GE
		/// </summary>
		static public bool IsAlmostGE(this float value, float b)
		{
			return IsAlmostGE(value, b, Mathf.Epsilon);
		}
		/// <summary>
		/// GE
		/// </summary>
		static public bool IsAlmostGE(this float value, float b, float precision)
		{
			return b <= (value - precision);
		}

		/// <summary>
		/// LE
		/// </summary>
		static public bool IsAlmostLE(this float value, float b)
		{
			return IsAlmostLE(value, b, Mathf.Epsilon);
		}
		/// <summary>
		/// LE
		/// </summary>
		static public bool IsAlmostLE(this float value, float b, float precision)
		{
			return (value - precision) <= b;
		}


		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// 除算
		/// </summary>
		static public double SafeDivide( this double num, double denom, double valueWhenError = default )
		{
			return IsZero( denom ) ? valueWhenError : num / denom;
		}
		
		/// <summary>
		/// 剰余
		/// </summary>
		static public double Mod(this double a, double b)
		{
			return a - Math.Floor(a / b) * b;
		}

		/// <summary>
		/// Zero チェック
		/// </summary>
		static public bool IsZero(this double value)
		{
			return Math.Abs(value) <= Mathf.Epsilon;
		}

		/// <summary>
		/// 比較
		/// </summary>
		static public bool IsAlmostEqual(this double value, double b)
		{
			return IsAlmostEqual(value, b, Mathf.Epsilon);
		}
		/// <summary>
		/// 比較
		/// </summary>
		static public bool IsAlmostEqual(this double value, double b, double precision)
		{
			return Math.Abs(value - b) <= precision;
		}

		/// <summary>
		/// GE
		/// </summary>
		static public bool IsAlmostGE(this double value, double b)
		{
			return IsAlmostGE(value, b, Mathf.Epsilon);
		}
		/// <summary>
		/// GE
		/// </summary>
		static public bool IsAlmostGE(this double value, double b, double precision)
		{
			return b <= (value - precision);
		}

		/// <summary>
		/// LE
		/// </summary>
		static public bool IsAlmostLE(this double value, double b)
		{
			return IsAlmostLE(value, b, Mathf.Epsilon);
		}
		/// <summary>
		/// LE
		/// </summary>
		static public bool IsAlmostLE(this double value, double b, double precision)
		{
			return (value - precision) <= b;
		}

	}
}
