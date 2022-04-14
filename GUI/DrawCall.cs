using DirectDimensional.Bindings.WinAPI;

namespace DirectDimensional.Editor.GUI {
    public unsafe struct DrawCall {
        public uint IndexCount;
        public uint IndexLocation;

        public IntPtr TexturePointer;
        public IntPtr SamplerPointer;

        public RECT ScissorsRect = new() { Left = 0, Top = 0, Right = 10000, Bottom = 10000 };
    }
}
