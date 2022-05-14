using DirectDimensional.Bindings.WinAPI;
using DirectDimensional.Bindings.Direct3D11;
using DirectDimensional.Bindings;

namespace DirectDimensional.Editor.GUI {
    public struct DrawCall {
        public uint IndexCount;
        public uint IndexLocation;

        public D3D11_PRIMITIVE_TOPOLOGY Topology = D3D11_PRIMITIVE_TOPOLOGY.TriangleList;

        public IntPtr TexturePointer;
        public IntPtr SamplerPointer;

        //public IntPtr VertexShader;
        //public IntPtr PixelShader;

        public RECT ScissorsRect = new() { Left = -10000, Top = -10000, Right = 10000, Bottom = 10000 };
    }
}
