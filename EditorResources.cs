using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using DirectDimensional.Core;
using StbTrueTypeSharp;
using DirectDimensional.Bindings;
using DirectDimensional.Bindings.Direct3D11;
using DirectDimensional.Bindings.DXGI;

using static StbTrueTypeSharp.StbTrueType;
using D3D11Texture2D = DirectDimensional.Bindings.Direct3D11.Texture2D;

namespace DirectDimensional.Editor {
    internal static unsafe class EditorResources {
        public const float FontPixelHeight = 14;

        public static D3D11Texture2D FontBitmap { get; private set; } = null!;
        public static ShaderResourceView FontTextureView { get; private set; } = null!;
        public static stbtt_fontinfo FontInfo { get; private set; }

        public static stbtt_packedchar[] PackedChar { get; private set; }

        static EditorResources() {
            FontInfo = new();
            PackedChar = new stbtt_packedchar[128];
        }

        public static bool LoadResources() {
            IntPtr pFontBitmap = IntPtr.Zero;

            try {
                var fontPath = Path.Combine(Editor.ApplicationDirectory, "Resources", "Roboto-Light.ttf");

                var ttf = File.ReadAllBytes(fontPath);
                fixed (byte* pTTF = &ttf[0]) {
                    if (stbtt_InitFont(FontInfo, pTTF, 0) == 0) {
                        throw new Exception("Cannot Initialize ttf font");
                    }

                    int bitmapWidth = 512;
                    int bitmapHeight = 256;

                    pFontBitmap = Marshal.AllocHGlobal(bitmapWidth * bitmapHeight * 4);
                    Unsafe.InitBlock(pFontBitmap.ToPointer(), 0x00, (uint)(bitmapWidth * bitmapHeight * 4));

                    stbtt_pack_context packContent = new();
                    stbtt_PackBegin(packContent, (byte*)pFontBitmap.ToPointer(), bitmapWidth, bitmapHeight, 0, 1, null);
                    fixed (stbtt_packedchar* ptr = &PackedChar[0]) {
                        stbtt_PackSetOversampling(packContent, 3, 1);
                        stbtt_PackFontRange(packContent, pTTF, 0, FontPixelHeight, 32, 95, ptr + 32);
                    }
                    stbtt_PackEnd(packContent);

                    for (int i = bitmapWidth * bitmapHeight - 1; i >= 0; i--) {
                        var b = Marshal.ReadByte(pFontBitmap, i);
                        Marshal.WriteInt32(pFontBitmap, i * 4, new Color32(255, 255, 255, b).Integer);
                    }

                    D3D11_TEXTURE2D_DESC desc = default;
                    desc.CPUAccessFlags = D3D11_CPU_ACCESS_FLAG.None;
                    desc.Usage = D3D11_USAGE.Default;
                    desc.MipLevels = desc.ArraySize = 1u;
                    desc.Format = DXGI_FORMAT.R8G8B8A8_UNORM;
                    desc.BindFlags = D3D11_BIND_FLAG.ShaderResource;
                    desc.SampleDesc.Count = 1u;
                    desc.Width = (uint)bitmapWidth;
                    desc.Height = (uint)bitmapHeight;
                    desc.MiscFlags = D3D11_RESOURCE_MISC_FLAG.None;

                    D3D11_SUBRESOURCE_DATA srd = default;
                    srd.pData = pFontBitmap;
                    srd.SysMemPitch = (uint)(bitmapWidth * 4);

                    var device = Direct3DContext.Device;

                    device.CreateTexture2D(desc, &srd, out var texture).ThrowExceptionIfError();
                    FontBitmap = texture!;

                    Marshal.FreeHGlobal(pFontBitmap);

                    D3D11_SHADER_RESOURCE_VIEW_DESC sdesc = default;
                    sdesc.Format = DXGI_FORMAT.R8G8B8A8_UNORM;
                    sdesc.ViewDimension = D3D11_SRV_DIMENSION.Texture2D;
                    sdesc.Texture2D.MipLevels = 1;

                    Direct3DContext.Device.CreateShaderResourceView(FontBitmap, &sdesc, out var fontView).ThrowExceptionIfError();
                    FontTextureView = fontView!;
                }

                return true;
            } catch (Exception e) {
                Logger.Error(e.ToString());
                if (pFontBitmap != IntPtr.Zero) Marshal.FreeHGlobal(pFontBitmap);

                return false;
            }
        }

        public static void Shutdown() {
            FontBitmap.CheckAndRelease();
            FontTextureView.CheckAndRelease();
        }
    }
}
