using System.Text;
using System.Runtime.InteropServices;
using DirectDimensional.Bindings.WinAPI;
using DirectDimensional.Bindings.DXGI;
using DirectDimensional.Bindings.Direct3D11;
using DirectDimensional.Bindings;
using DirectDimensional.Core;
using DirectDimensional.Core.Miscs;
using System.Diagnostics;

using D3DTexture2D = DirectDimensional.Bindings.Direct3D11.Texture2D;

namespace DirectDimensional.Editor {
    internal static unsafe class EditorApplication {
        public static event Action? OnResize;

        private static readonly Stopwatch _stopwatch = new();

        public static int FrameCount { get; private set; }
        public static double ElapsedTime => _stopwatch.Elapsed.TotalSeconds;

        private static float _deltaTime;
        public static float DeltaTime => _deltaTime;

        public static unsafe int Main() {
            if (!Environment.Is64BitOperatingSystem) {
                WinAPI.MessageBoxW(IntPtr.Zero, "Operation System isn't 64 bit", "DirectDimensional: Invalid Architecture", 0x00000030);
                return -1;
            }

            StandardShaderInclude.UpdateInstance(@"D:\C# Projects\DirectDimensional.Core\Resources\", @"D:\C# Projects\DirectDimensional.Core\Resources\");

            IntPtr hInstance = Process.GetCurrentProcess().Handle;

            WNDCLASSEXW classEx = default;
            classEx.cbSize = (uint)Marshal.SizeOf<WNDCLASSEXW>();
            classEx.lpszClassName = "Main Window";
            classEx.lpfnWndProc = WndProc;
            classEx.style = 0x20;   // CS_OWNDC
            classEx.hInstance = hInstance;
            classEx.hCursor = WinAPI.LoadCursorW(IntPtr.Zero, StandardCursorID.Arrow);

            WinAPI.RegisterClassExW(ref classEx);

            const uint overlappedWindow = 13565952U;    // Standard window style that will always be used (not because I'm lazy). Correspond to WS_OVERLAPPEDWINDOW
            EditorWindow.WindowHandle = WinAPI.CreateWindowExW(0, "Main Window", "DirectDimensional", overlappedWindow, 0, 0, 800, 600, IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);
            WinAPI.ShowWindow(EditorWindow.WindowHandle, 3);

            // Initialize raw mouse input
            {
                RAWINPUTDEVICE rid = default;
                rid.UsagePage = 0x01;
                rid.Usage = 0x02;
                rid.Flags = 0;
                rid.hwndTarget = IntPtr.Zero;

                if (!WinAPI.RegisterRawInputDevices(&rid, 1, (uint)sizeof(RAWINPUTDEVICE))) {
                    throw Marshal.GetExceptionForHR(Marshal.GetLastWin32Error())!;
                }
            }

            Logger.LoggingEvent += LoggerLog;
            Logger.WarningEvent += LoggerWarn;
            Logger.ErrorEvent += LoggerError;

            Direct3DContext.Initialize(EditorWindow.WindowHandle);
            EditorLifecycle.Initialize();

            int exitCode;
            _stopwatch.Start();

            while (true) {
                float dts = (float)_stopwatch.Elapsed.TotalSeconds;
                if (ProcessMessage(out exitCode)) break;

                var devctx = Direct3DContext.DevCtx;

                devctx.ClearRenderTargetView(Direct3DContext.RenderingOutput, new(0.06f, 0.06f, 0.06f, 1));
                devctx.OMSetRenderTargets(Direct3DContext.RenderingOutputAsArray, null);

                EditorLifecycle.Cycle();

                Direct3DContext.SwapChain.Present(1u, DXGI_PRESENT.None);

                _deltaTime = (float)_stopwatch.Elapsed.TotalSeconds - dts;
                FrameCount++;

                Direct3DContext.FlushD3D11DebugMessages(DebugWriter);
                Direct3DContext.ClearD3D11DebugMessages();
            }

            EditorLifecycle.Shutdown();

            return exitCode;
        }

        private static readonly StringBuilder loggerSB = new(1024);
        private static void LoggerLog(object? obj, StackTrace? stacktrace) {
            loggerSB.Clear();
            loggerSB.Append(obj);

            if (stacktrace != null) {
                loggerSB.AppendLine();
                Logger.DecodeStacktrace(stacktrace, loggerSB);
            }

            Console.Write(loggerSB.ToString());
        }
        private static void LoggerWarn(object? obj, StackTrace? stacktrace) {
            loggerSB.Clear();
            loggerSB.Append(obj);

            if (stacktrace != null) {
                loggerSB.AppendLine();
                Logger.DecodeStacktrace(stacktrace, loggerSB);
            }

            var col = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(loggerSB.ToString());
            Console.ForegroundColor = col;
        }
        private static void LoggerError(object? obj, StackTrace? stacktrace) {
            loggerSB.Clear();
            loggerSB.Append(obj);

            if (stacktrace != null) {
                loggerSB.AppendLine();
                Logger.DecodeStacktrace(stacktrace, loggerSB);
            }

            var col = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(loggerSB.ToString());
            Console.ForegroundColor = col;
        }

        private static unsafe void DebugWriter(IntPtr pMessage) {
            D3D11_MESSAGE* pMsg = (D3D11_MESSAGE*)pMessage;

            if (pMsg->Category != D3D11_MESSAGE_CATEGORY.D3D11_MESSAGE_CATEGORY_STATE_CREATION && pMsg->Severity != D3D11_MESSAGE_SEVERITY.D3D11_MESSAGE_SEVERITY_INFO) {
                Logger.Log(pMsg->Description + " [" + pMsg->Severity + " - " + pMsg->ID + "]");
            }
        }

        internal static bool ProcessMessage(out int exit) {
            Mouse.UpdateLastStates();
            Keyboard.UpdateLastStates();

            while (WinAPI.PeekMessageW(out var msg, IntPtr.Zero, 0, 0, 1)) {
                if (msg.Message == WindowMessages.Quit) {
                    exit = (int)msg.wParam;
                    return true;
                }

                WinAPI.TranslateMessage(ref msg);
                WinAPI.DispatchMessageW(ref msg);
            }

            exit = 0;
            return false;
        }

        internal static nint WndProc(IntPtr hwnd, WindowMessages msg, nuint wParam, nint lParam) {
            var wndSize = EditorWindow.ClientSize;

            switch (msg) {
                case WindowMessages.Destroy:    // WM_DESTROY
                    WinAPI.PostQuitMessage(0);
                    return 0;

                case WindowMessages.Size:
                    OnResize?.Invoke();

                    Direct3DContext.ResizeSwapChain();
                    break;

                // User inputs
                case WindowMessages.KeyDown:
                case WindowMessages.SysKeyDown:
                    Keyboard.RegisterKeyDown((byte)wParam);
                    break;

                case WindowMessages.KeyUp:
                case WindowMessages.SysKeyUp:
                    Keyboard.RegisterKeyRelease((byte)wParam);
                    break;

                case WindowMessages.MouseMove: {
                    POINTS p = WinAPI.MakePOINTS(lParam);

                    if (p.X >= 0 && p.X < wndSize.X && p.Y >= 0 && p.Y < wndSize.Y) {
                        Mouse.RegisterMouseMove(p.X, p.Y);

                        if (!Mouse.InsideWindow) {
                            WinAPI.SetCapture(EditorWindow.WindowHandle);
                            Mouse.RegisterMouseEnterWnd();
                        }
                    } else {
                        // Check for MK_LBUTTON and MK_RBUTTON is down
                        if ((wParam & (0x0001 | 0x0002)) != 0) {
                            Mouse.RegisterMouseMove(p.X, p.Y);
                        } else {
                            WinAPI.ReleaseCapture();
                            Mouse.RegisterMouseLeaveWnd();
                        }
                    }

                    Mouse.RegisterMouseMove(p.X, p.Y);
                    break;
                }

                case WindowMessages.MouseWheel:
                    Mouse.RegisterMouseWheel(Math.Sign((short)WinAPI.HIWORD((int)wParam)));
                    break;

                case WindowMessages.LMouseDown:
                    Mouse.RegisterLMouseDown();
                    WinAPI.SetForegroundWindow(EditorWindow.WindowHandle);
                    break;

                case WindowMessages.LMouseUp: {
                    Mouse.RegisterLMouseUp();

                    POINTS p = WinAPI.MakePOINTS(lParam);
                    if (p.X < 0 || p.X >= wndSize.X || p.Y < 0 || p.Y >= wndSize.Y) {
                        WinAPI.ReleaseCapture();
                        Mouse.RegisterMouseLeaveWnd();
                    }
                    break;
                }

                case WindowMessages.RMouseDown:
                    Mouse.RegisterRMouseDown();
                    WinAPI.SetForegroundWindow(EditorWindow.WindowHandle);
                    break;

                case WindowMessages.RMouseUp: {
                    Mouse.RegisterRMouseUp();

                    POINTS p = WinAPI.MakePOINTS(lParam);
                    if (p.X < 0 || p.X >= wndSize.X || p.Y < 0 || p.Y >= wndSize.Y) {
                        WinAPI.ReleaseCapture();
                        Mouse.RegisterMouseLeaveWnd();
                    }
                    break;
                }

                case WindowMessages.MMouseDown:
                    Mouse.RegisterMMouseDown();
                    WinAPI.SetForegroundWindow(EditorWindow.WindowHandle);
                    break;

                case WindowMessages.MMouseUp: {
                    Mouse.RegisterMMouseUp();

                    POINTS p = WinAPI.MakePOINTS(lParam);
                    if (p.X < 0 || p.X >= wndSize.X || p.Y < 0 || p.Y >= wndSize.Y) {
                        WinAPI.ReleaseCapture();
                        Mouse.RegisterMouseLeaveWnd();
                    }
                    break;
                }

                case WindowMessages.Input: {
                    uint size = 48;
                    RAWINPUT input = default;

                    if (WinAPI.GetRawInputData((RAWINPUT*)lParam, 0x10000003, &input, ref size, sizeof(RAWINPUTHEADER)) != size) break;

                    if (input.Header.Type == 0) {
                        if (input.Mouse.LastX != 0 || input.Mouse.LastY != 0) {
                            Mouse.RegisterMouseRaw(input.Mouse.LastX, input.Mouse.LastY);
                        }
                    }

                    return 0;
                }
            }

            return WinAPI.DefWindowProcW(hwnd, msg, wParam, lParam);
        }
    }
}
