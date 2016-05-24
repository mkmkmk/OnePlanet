using System;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace NoToolkitDxLib
{
    public class MyMesh<TVector> : IDisposable where TVector : struct
    {
        private readonly Buffer _vertices;
        private readonly Buffer _indices;
        private readonly int _stride;

        private readonly int _indicesCount;
        private readonly Device _device;

        public MyMesh(Device device, TVector[] sphereVert, ushort[] indices, int stride)
        {
            _device = device;
            _stride = stride;

            _vertices = Buffer.Create(device, BindFlags.VertexBuffer, sphereVert);
            _indices = Buffer.Create(device, BindFlags.IndexBuffer, indices);
            _indicesCount = indices.Length;
        }


        public void Dispose()
        {
            _vertices.Dispose();
            _indices.Dispose();
        }

        public void Draw()
        {
            var context = _device.ImmediateContext;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertices, Utilities.SizeOf<TVector>() * _stride, 0));
            context.InputAssembler.SetIndexBuffer(_indices, SharpDX.DXGI.Format.R16_UInt, 0);
            context.DrawIndexed(_indicesCount, 0, 0);
        }
    }
}