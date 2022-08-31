using UnityEngine;

namespace OpenKh.Unity.Exporter.Aset.Utils
{
    public static class MatrixUtil
    {
        /// <summary>
        /// Scales a Matrix4x4 by a given factor
        /// </summary>
        /// <param name="m">The matrix to scale</param>
        /// <param name="scale">Scale factor</param>
        /// <returns>The specified matrix scaled by the given factor as a new Matrix4x4</returns>
        public static Matrix4x4 Scale(Matrix4x4 m, float scale)
        {
            return new Matrix4x4
            {
                m00 = m.m00 * scale,
                m01 = m.m01 * scale,
                m02 = m.m02 * scale,
                m03 = m.m03 * scale,
                m10 = m.m10 * scale,
                m11 = m.m11 * scale,
                m12 = m.m12 * scale,
                m13 = m.m13 * scale,
                m20 = m.m20 * scale,
                m21 = m.m21 * scale,
                m22 = m.m22 * scale,
                m23 = m.m23 * scale,
                m30 = m.m30 * scale,
                m31 = m.m31 * scale,
                m32 = m.m32 * scale,
                m33 = m.m33 * scale,
            };
        }

        /// <summary>
        /// Sums two matrices component-wise
        /// </summary>
        /// <param name="lhs">First addend</param>
        /// <param name="rhs">Second addend</param>
        /// <returns>The sum of the two matrices as a new Matrix4x4</returns>
        public static Matrix4x4 Sum(Matrix4x4 lhs, Matrix4x4 rhs)
        {
            return new Matrix4x4
            {
                m00 = lhs.m00 + rhs.m00,
                m01 = lhs.m01 + rhs.m01,
                m02 = lhs.m02 + rhs.m02,
                m03 = lhs.m03 + rhs.m03,
                m10 = lhs.m10 + rhs.m10,
                m11 = lhs.m11 + rhs.m11,
                m12 = lhs.m12 + rhs.m12,
                m13 = lhs.m13 + rhs.m13,
                m20 = lhs.m20 + rhs.m20,
                m21 = lhs.m21 + rhs.m21,
                m22 = lhs.m22 + rhs.m22,
                m23 = lhs.m23 + rhs.m23,
                m30 = lhs.m30 + rhs.m30,
                m31 = lhs.m31 + rhs.m31,
                m32 = lhs.m32 + rhs.m32,
                m33 = lhs.m33 + rhs.m33,
            };
        }
    }
}
