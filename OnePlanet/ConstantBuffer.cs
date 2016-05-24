using System.Runtime.InteropServices;
using SharpDX;
using Matrix = SharpDX.Matrix;

namespace OnePlanet
{
    [StructLayout(LayoutKind.Sequential, Size = 64 + 64 + 16 + 16)]
    struct ConstantBuffer
    {
        public Matrix WorldViewProj;
        public Matrix World;
        public Vector4 LightDir;
        public int Light;
    }
}