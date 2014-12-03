using UnityEngine;
using System.Collections;

public static class Utils  {

	public static float cross(this Vector2 vec, Vector2 vec2) {
		return vec.x*vec2.y - vec.y*vec2.x;
	}

	public static Vector2 cross(this Vector2 vec, float s) {
		return new Vector2(-s * vec.y, s * vec.x);
	}

	public static Vector2 Abs(Vector2 a)
	{
		return new Vector2(Mathf.Abs(a.x), Mathf.Abs(a.y));
	}

	public static Matrix22 Abs(Matrix22 A)
	{
		return new Matrix22(Utils.Abs(A.col1), Utils.Abs(A.col2));
	}

	public static float AngleSigned(Vector3 from, Vector3 to) {
		float angle = Mathf.Acos(Mathf.Clamp (Vector3.Dot (from.normalized, to.normalized), -1f, 1f));
		return angle * Mathf.Sign(Vector3.Cross(from, to).y);
	}

	public static float AngleSigned(Vector2 from, Vector2 to) {
		float angle = Mathf.Acos(Mathf.Clamp (Vector2.Dot (from.normalized, to.normalized), -1f, 1f));
		return angle * -Mathf.Sign(from.cross(to));
	}

	public static float AngleSigned(Vector2 vec) {
		float res = Mathf.Atan2(vec.y, vec.x)*Mathf.Rad2Deg;
		if (vec.y < 0f)
			res = 360f + res;
		return res;
	}

	public static void Swap<T>(ref T a, ref T b)
	{
		T tmp = a;
		a = b;
		b = tmp;
	}

	public static Vector2 InterpolateBezier(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float coef)
	{
		float m = 1 - coef;
		float n = m*m;
		float o = n*m;
		float p = coef*coef;
		float r = p*coef;

		return a*o + b*3.0f*coef*n + c*3.0f*p*m + d*r;
	}

	public static void SetTransformToLine(Transform transf, float length, Vector2 a, Vector2 b) {
		transf.position = a;
		transf.eulerAngles = Vector3.forward*Utils.AngleSigned(a - b, Vector2.up)*Mathf.Rad2Deg;
		transf.localScale = new Vector3(1, (b - a).magnitude/length, 1);
	}

	public static Vector2 RotatedVector2(float angleDeg, float length = 1f) {
		return new Vector2(Mathf.Cos(angleDeg*Mathf.Deg2Rad), Mathf.Sin(angleDeg*Mathf.Deg2Rad))*length;
	}

	public static Vector2 RotateDeg(this Vector2 vec, float angle) {
		float rad = angle*Mathf.Deg2Rad;
		float cs = Mathf.Cos(rad), sn = Mathf.Sin(rad);
		return new Vector2(cs*vec.x - sn*vec.y, sn*vec.x + cs*vec.y);
	}

	public static void TransformFromMatrix(Matrix4x4 matrix, Transform trans) { 
		trans.position = matrix.GetColumn(3);
 
		// Extract new local rotation
		trans.rotation = Quaternion.LookRotation(matrix.GetColumn(2),
		                                         matrix.GetColumn(1));
 
		// Extract new local scale
		trans.localScale = new Vector3(matrix.GetColumn(0).magnitude,
		                               matrix.GetColumn(1).magnitude,
		                               matrix.GetColumn(2).magnitude);
	}
}

public class Matrix22
{
	public Vector2 col1, col2;

	public Matrix22() {}
	public Matrix22(float angle)
	{
		float c = Mathf.Cos(angle), s = Mathf.Sin(angle);
		col1.x = c; col2.x = -s;
		col1.y = s; col2.y = c;
	}

	public Matrix22(Vector2 col1, Vector2 col2)
	{
		this.col1 = col1;
		this.col2 = col2;
	}

	public Matrix22 Transpose()
	{
		return new Matrix22(new Vector2(col1.x, col2.x), new Vector2(col1.y, col2.y));
	}

	public Matrix22 Invert()
	{
		float a = col1.x, b = col2.x, c = col1.y, d = col2.y;
		Matrix22 B = new Matrix22();
		float det = a * d - b * c;

		if (det == 0.0f)
			return B;

		det = 1.0f / det;
		B.col1.x =  det * d;	B.col2.x = -det * b;
		B.col1.y = -det * c;	B.col2.y =  det * a;
		return B;
	}

	public static Vector2 operator*(Matrix22 A, Vector2 v) {
		return new Vector2(A.col1.x * v.x + A.col2.x * v.y, A.col1.y * v.x + A.col2.y * v.y);
	}

	public static Matrix22 operator*(Matrix22 A, Matrix22 B) {
		return new Matrix22(A * B.col1, A * B.col2);
	}

	public static Matrix22 operator+(Matrix22 A, Matrix22 B) {
		return new Matrix22(A.col1 + B.col1, A.col2 + B.col2);
	}
};
