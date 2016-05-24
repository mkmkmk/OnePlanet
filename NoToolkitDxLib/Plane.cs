using System;
using System.Linq;
using SharpDX;

namespace NoToolkitDxLib
{
    public class Plane
    {

        public VertexPositionNormalTexture[] Vertices { get; private set; }
        public ushort[] Indices { get; private set; }

        public Plane(float sizeX = 1f, float sizeY = 1f, int tessellation = 1, bool toLeftHanded = false)
            : this(sizeX, sizeY, tessellation, Vector2.One, toLeftHanded)
        {

        }

        public Plane(float sizeX, float sizeY, int tessellation, Vector2 uvFactor, bool toLeftHanded = false)
        {
            if (tessellation < 1)
                throw new ArgumentOutOfRangeException("tessellation", "tessellation must be > 0");
            int num1 = tessellation + 1;
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[num1 * num1];
            int[] indices = new int[tessellation * tessellation * 6];
            float num2 = sizeX / (float)tessellation;
            float num3 = sizeY / (float)tessellation;
            sizeX /= 2f;
            sizeY /= 2f;
            int num4 = 0;
            int num5 = 0;
            Vector3 normal = Vector3.UnitZ;
            for (int index1 = 0; index1 < tessellation + 1; ++index1)
            {
                for (int index2 = 0; index2 < tessellation + 1; ++index2)
                {
                    Vector3 position = new Vector3((float)(-(double)sizeX + (double)num2 * (double)index2), sizeY - num3 * (float)index1, 0.0f);
                    Vector2 textureCoordinate = new Vector2(1f * (float)index2 / (float)tessellation * uvFactor.X, 1f * (float)index1 / (float)tessellation * uvFactor.Y);
                    vertices[num4++] = new VertexPositionNormalTexture(position, normal, textureCoordinate);
                }
            }
            for (int index1 = 0; index1 < tessellation; ++index1)
            {
                for (int index2 = 0; index2 < tessellation; ++index2)
                {
                    int num6 = num1 * index1 + index2;
                    int[] numArray1 = indices;
                    int index3 = num5;
                    int num7 = 1;
                    int num8 = index3 + num7;
                    int num9 = num6 + 1;
                    numArray1[index3] = num9;
                    int[] numArray2 = indices;
                    int index4 = num8;
                    int num10 = 1;
                    int num11 = index4 + num10;
                    int num12 = num6 + 1 + num1;
                    numArray2[index4] = num12;
                    int[] numArray3 = indices;
                    int index5 = num11;
                    int num13 = 1;
                    int num14 = index5 + num13;
                    int num15 = num6 + num1;
                    numArray3[index5] = num15;
                    int[] numArray4 = indices;
                    int index6 = num14;
                    int num16 = 1;
                    int num17 = index6 + num16;
                    int num18 = num6 + 1;
                    numArray4[index6] = num18;
                    int[] numArray5 = indices;
                    int index7 = num17;
                    int num19 = 1;
                    int num20 = index7 + num19;
                    int num21 = num6 + num1;
                    numArray5[index7] = num21;
                    int[] numArray6 = indices;
                    int index8 = num20;
                    int num22 = 1;
                    num5 = index8 + num22;
                    int num23 = num6;
                    numArray6[index8] = num23;
                }
            }
            Vertices = vertices;
            Indices = indices.Select(el => (ushort)el).ToArray();
            if (toLeftHanded)
            {
                for (int i1 = 0; i1 < Indices.Length; i1 += 3)
                {
                    Utilities.Swap(ref Indices[i1], ref Indices[i1 + 2]);
                }
            }
        }
    }
}