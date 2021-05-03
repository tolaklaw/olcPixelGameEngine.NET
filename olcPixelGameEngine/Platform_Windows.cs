/*
#pragma comment(lib, "user32.lib")		// Visual Studio Only
#pragma comment(lib, "gdi32.lib")		// For other Windows Compilers please add
#pragma comment(lib, "opengl32.lib")	// these libs to your linker input
*/


using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Text;
using static olc.Const;
//using static olc.Platform_Helper;

namespace olc
{
    public unsafe class Platform_Windows : Platform
    {
        public Platform_Windows()
        {
            
        }

        //Platform_Helper.


        Window* olc_hWnd = null;
        string wsAppName;

        public override rcode ApplicationCleanUp() => rcode.OK;
        public override rcode ApplicationStartUp() => rcode.OK;

        public override rcode CreateGraphics(bool bFullScreen, bool bEnableVSYNC, vi2d vViewPos, vi2d vViewSize)
        {
            renderer.SetWindowObject(olc_hWnd);
            if (renderer.CreateDevice(null, bFullScreen, bEnableVSYNC) == rcode.OK)
            {
                renderer.UpdateViewport(vViewPos, vViewSize);
                return rcode.OK;
            }
            else
                return rcode.FAIL; ;
        }
        //WndProc del;

        public override rcode CreateWindowPane(vi2d vWindowPos, vi2d vWindowSize, bool bFullScreen)
        {
            //Console.WriteLine("CreateWindowPane-start");
            //WNDCLASS wc = new WNDCLASS();
            //wc.hIcon = LoadIcon(IntPtr.Zero, IDI_APPLICATION); // IDI_APPLICATION (Default icon)
            //wc.hCursor = LoadCursor(IntPtr.Zero, IDC_ARROW); // IDC_ARROW (Standard arrow)
            //wc.style = ClassStyles.HorizontalRedraw | ClassStyles.VerticalRedraw | ClassStyles.OwnDC;
            //wc.hInstance = GetModuleHandle(null);
            //wc.lpfnWndProc = del;
            //wc.cbClsExtra = 0;
            //wc.cbWndExtra = 0;
            //wc.lpszMenuName = null;
            //wc.hbrBackground = IntPtr.Zero;
            //wc.lpszClassName = "OLC_PIXEL_GAME_ENGINE";
            //RegisterClass(ref wc);

            // Define window furniture
            //var dwExStyle =  WindowStylesEx.WS_EX_APPWINDOW | WindowStylesEx.WS_EX_WINDOWEDGE;            
            //var dwStyle = WindowStyles.WS_CAPTION | WindowStyles.WS_SYSMENU | WindowStyles.WS_VISIBLE | WindowStyles.WS_THICKFRAME;

            vi2d vTopLeft = vWindowPos;

            // Handle Fullscreen
            //if (bFullScreen)
            //{
            //    dwExStyle = WindowStylesEx.WS_EX_LEFT; 
            //    dwStyle = WindowStyles.WS_VISIBLE | WindowStyles.WS_POPUP;
            //    IntPtr hmon = MonitorFromWindow(olc_hWnd, MONITOR_DEFAULTTONEAREST);
            //    MonitorInfoEx mi = new MonitorInfoEx(); /*= { sizeof(mi) };*/
            //    if (!GetMonitorInfo(hmon, ref mi)) return rcode.FAIL;
            //    vWindowSize = new vi2d( mi.Monitor.Right, mi.Monitor.Bottom);
            //    vTopLeft.x = 0;
            //    vTopLeft.y = 0;
            //}

            // Keep client size as requested
            //RECT rWndRect = new RECT ( 0, 0, vWindowSize.x, vWindowSize.y );
            //AdjustWindowRectEx(ref rWndRect, (uint)dwStyle, false, (uint)dwExStyle);
            int width = vWindowSize.x;//rWndRect.Right - rWndRect.Left;
            int height = vWindowSize.y; //rWndRect.Bottom - rWndRect.Top;


            //olc_hWnd = CreateWindowEx(dwExStyle, "OLC_PIXEL_GAME_ENGINE", "", dwStyle,  // fix dwExStyle
            //    vTopLeft.x, vTopLeft.y, width, height, IntPtr.Zero, IntPtr.Zero, GetModuleHandle(null), IntPtr.Zero); //  lpParam should have been set to "this" ?
            GLFW.Init();
            GLFW.WindowHint(WindowHintInt.ContextVersionMajor, 3);
            GLFW.WindowHint(WindowHintInt.ContextVersionMinor, 3);
            GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
            GLFW.WindowHint(WindowHintBool.DoubleBuffer, true);
            olc_hWnd = GLFW.CreateWindow(width, height, "OLC_PIXEL_GAME_ENGINE", null, null);

            GLFW.SetFramebufferSizeCallback(olc_hWnd, framebufferCallback);
            GLFW.SetErrorCallback(errorCallback);
            GLFW.SetMouseButtonCallback(olc_hWnd, mousebuttonCallback);
            GLFW.SetScrollCallback(olc_hWnd, scrollCallback);
            GLFW.SetCursorPosCallback(olc_hWnd, cursorposCallback);
            keyCallback = key_Callback;
            GLFW.SetKeyCallback(olc_hWnd, keyCallback);
            //GLFW.MakeContextCurrent((Window*)olc_hWnd);

            //GLFW.MakeContextCurrent(olc_hWnd);

            //if (olc_hWnd == null)
            //{
            //    Console.WriteLine($"LastError : [{Marshal.GetLastWin32Error()}]");
            //}
            Console.WriteLine("Window Created");
            Console.WriteLine("CreateWindowPane-end");
            return rcode.OK;
        }

        GLFWCallbacks.ErrorCallback errorCallback = Platform_Windows.error_callback;
        GLFWCallbacks.FramebufferSizeCallback framebufferCallback = Platform_Windows.framebuffer_size_callback;
        GLFWCallbacks.KeyCallback keyCallback;
        GLFWCallbacks.MouseButtonCallback mousebuttonCallback = mousebutton_Callback;
        GLFWCallbacks.ScrollCallback scrollCallback = scroll_Callback;
        GLFWCallbacks.CursorPosCallback cursorposCallback = cursorpos_Callback;


        private static void error_callback(ErrorCode error, string description)
        {
            Console.WriteLine($"Error [[{error}]{description}]");
        }

        static void framebuffer_size_callback(Window* window, int width, int height)
        {
            //GL.Viewport(0, 0, width, height);
            Platform.PGE.olc_UpdateWindowSize(width, height);
        }

        public override rcode HandleSystemEvent()
        {
            //  GLFW.PollEvents();
            //GLFW.PollEvents();
            return rcode.OK;
        }

        public override rcode SetWindowTitle(string s)
        {
            GLFW.SetWindowTitle(olc_hWnd, s);
           // GLFW.PollEvents();
            //SetWindowText(olc_hWnd, s);
            return rcode.OK;

        }

        public override rcode StartSystemEventLoop()
        {
            //MSG msg;
            Console.WriteLine("Getting Messages");
            int ret;
            while (!GLFW.WindowShouldClose(olc_hWnd))
            {
                GLFW.WaitEvents();
                //GLFW.PollEvents();
            }
            PGE.olc_Terminate();
            //GLFW.Terminate();
            return rcode.OK;
            //return rcode.OK;
//            while ((ret = GetMessage(out msg, IntPtr.Zero, 0, 0)) != 0)
//            {
                
//                if (ret == -1)
//                {
//                    Console.WriteLine($"LastError : [{Marshal.GetLastWin32Error()}]");
//                }
//                else
//                {
                    
//  //                  TranslateMessage(ref msg);
////                    DispatchMessage(ref msg);
////                    Console.WriteLine($"msg: [t:{msg.time}] [wparam:{msg.wParam}] [lparam:{msg.lParam}] [message:{msg.message}] [msgmsg:{(WindowsMessage)msg.message}]");
//                }
                
//            }
//            return rcode.OK;

        }

        public override rcode ThreadCleanUp()
        {
            renderer.DestroyDevice();
            GLFW.Terminate();

            //var handle = new HandleRef(null, olc_hWnd); 
            //PostMessage(handle, (uint)WindowsMessage.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
            return rcode.OK;
        }

        public override rcode ThreadStartUp() => rcode.OK;

        private void key_Callback(Window* window, Keys key, int scanCode, InputAction action, KeyModifiers mods)
        {
            bool inputAction = false;
            if (action == InputAction.Press || action == InputAction.Repeat)
            {
                inputAction = true;
            }

            PGE.olc_UpdateKeyState((int)GLFWKeysToOLCKey(key),inputAction );
        }

        private olc.Key GLFWKeysToOLCKey(Keys key)
        {
            switch (key)
            {
                case Keys.Unknown: return Key.NONE;
                case Keys.Space: return Key.SPACE;
                case Keys.Comma: return Key.COMMA;                    
                case Keys.Minus: return Key.MINUS;
                case Keys.Period: return Key.PERIOD;
                case Keys.D0: return Key.K0;                    
                case Keys.D1: return Key.K1;                    
                case Keys.D2: return Key.K2;
                case Keys.D3: return Key.K3;
                case Keys.D4: return Key.K4;
                case Keys.D5: return Key.K5;
                case Keys.D6: return Key.K6;
                case Keys.D7: return Key.K7;
                case Keys.D8: return Key.K8;
                case Keys.D9: return Key.K9;
                case Keys.Equal: return Key.EQUALS;                    
                case Keys.A: return Key.A;
                case Keys.B: return Key.B;
                case Keys.C: return Key.C;
                case Keys.D: return Key.D;
                case Keys.E: return Key.E;
                case Keys.F: return Key.F;
                case Keys.G: return Key.G;
                case Keys.H: return Key.H;
                case Keys.I: return Key.I;
                case Keys.J: return Key.J;
                case Keys.K: return Key.K;
                case Keys.L: return Key.L;
                case Keys.M: return Key.M;
                case Keys.N: return Key.N;
                case Keys.O: return Key.O;
                case Keys.P: return Key.P;
                case Keys.Q: return Key.Q;
                case Keys.R: return Key.R;
                case Keys.S: return Key.S;
                case Keys.T: return Key.T;
                case Keys.U: return Key.U;
                case Keys.V: return Key.V;
                case Keys.W: return Key.W;
                case Keys.X: return Key.X;
                case Keys.Y: return Key.Y;
                case Keys.Z: return Key.Z;
                case Keys.Right: return Key.RIGHT;
                case Keys.Left: return Key.LEFT;
                case Keys.Down: return Key.DOWN;
                case Keys.Up: return Key.UP;
                case Keys.Escape: return Key.ESCAPE;
                case Keys.Tab: return Key.TAB;
                case Keys.LeftAlt: return Key.OEM_1;
                case Keys.RightAlt: return Key.OEM_2;
                case Keys.LeftSuper:
                case Keys.RightSuper: return Key.OEM_3;
                case Keys.LeftShift:
                case Keys.RightShift: return Key.SHIFT;
                case Keys.LeftControl:
                case Keys.RightControl: return Key.CTRL;
                case Keys.Delete: return Key.DEL;
                case Keys.PageUp: return Key.PGDN;
                case Keys.PageDown: return Key.PGUP;
                case Keys.Home: return Key.HOME;
                case Keys.End: return Key.END;
                case Keys.Backspace: return Key.BACK;


                case Keys.LeftBracket:
                case Keys.Backslash:
                case Keys.RightBracket:
                case Keys.GraveAccent:
                case Keys.Enter:
                case Keys.Insert:
                case Keys.CapsLock:
                case Keys.ScrollLock:
                case Keys.NumLock:
                case Keys.PrintScreen:
                case Keys.Pause:
                case Keys.F1:
                case Keys.F2:
                case Keys.F3:
                case Keys.F4:
                case Keys.F5:
                case Keys.F6:
                case Keys.F7:
                case Keys.F8:
                case Keys.F9:
                case Keys.F10:
                case Keys.F11:
                case Keys.F12:
                case Keys.F13:
                case Keys.F14:
                case Keys.F15:
                case Keys.F16:
                case Keys.F17:
                case Keys.F18:
                case Keys.F19:
                case Keys.F20:
                case Keys.F21:
                case Keys.F22:
                case Keys.F23:
                case Keys.F24:
                case Keys.F25:
                case Keys.KeyPad0:
                case Keys.KeyPad1:
                case Keys.KeyPad2:
                case Keys.KeyPad3:
                case Keys.KeyPad4:
                case Keys.KeyPad5:
                case Keys.KeyPad6:
                case Keys.KeyPad7:
                case Keys.KeyPad8:
                case Keys.KeyPad9:
                case Keys.KeyPadDecimal:
                case Keys.KeyPadDivide:
                case Keys.KeyPadMultiply:
                case Keys.KeyPadSubtract:
                case Keys.KeyPadAdd:
                case Keys.KeyPadEnter:
                case Keys.KeyPadEqual:
                case Keys.Menu:
                case Keys.Semicolon:
                case Keys.Slash:
                case Keys.Apostrophe:
                    return Key.NONE;
            }
            return Key.NONE;
        }


        private static void mousebutton_Callback(Window* window, MouseButton button, InputAction action, KeyModifiers mods)
        {
            int btn = -1;
            switch (button)
            {
                case MouseButton.Button1: btn = 0;  break;
                case MouseButton.Button2: btn = 1; break;
                case MouseButton.Button3: btn = 2; break;
                case MouseButton.Button4: btn = 3; break;
                case MouseButton.Button5: btn = 4; break;
                case MouseButton.Button6: btn = 5; break;
                case MouseButton.Button7: btn = 6; break;
                case MouseButton.Button8: btn = 7; break;
            }
            bool pressed = (action == InputAction.Press || action == InputAction.Repeat);
            PGE.olc_UpdateMouseState(btn, pressed);
        }


        private static void scroll_Callback(Window* window, double offsetX, double offsetY)
        {
            PGE.olc_UpdateMouseWheel((int)offsetY);
            //    //case WM_MOUSEWHEEL: ptrPGE->olc_UpdateMouseWheel(GET_WHEEL_DELTA_WPARAM(wParam)); return 0;
        }

        private static void cursorpos_Callback(Window* window, double x, double y)
        {
              PGE.olc_UpdateMouse((int)x, (int)y);
        }


        // Windows Event Handler - this is statically connected to the windows event system
        //public static IntPtr olc_WindowEvent(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        //{
        //if (msg != (uint)WindowsMessage.WM_SETTEXT)
        //{
        //    Console.WriteLine("--[olc_WindowEvent]--" + (WindowsMessage)msg + $" wParam:[{(long)wParam}]......lParam:[{(long)lParam}]");
        //}
        //switch (msg)
        //{
        //    //case (uint)WindowsMessage.WM_MOUSEMOVE:
        //    //    {

        //    //        // Thanks @ForAbby (Discord)
        //    //        ushort x = (ushort)((int)lParam & 0xFFFF);
        //    //        ushort y = (ushort)(((int)lParam >> 16) & 0xFFFF);
        //    //        ushort ix = x;
        //    //        ushort iy = y;
        //    //        PGE.olc_UpdateMouse(ix, iy);
        //    //        return IntPtr.Zero;
        //    //    }
        //    //case (uint)WindowsMessage.WM_SIZE: Platform.PGE.olc_UpdateWindowSize((int)lParam & 0xFFFF, ((int)lParam >> 16) & 0xFFFF); return IntPtr.Zero;

        //    //case WM_MOUSELEAVE: ptrPGE->olc_UpdateMouseFocus(false); return 0;
        //    //case WM_SETFOCUS: ptrPGE->olc_UpdateKeyFocus(true); return 0;
        //    //case WM_KILLFOCUS: ptrPGE->olc_UpdateKeyFocus(false); return 0;
        //        //    //case WM_KEYDOWN: ptrPGE->olc_UpdateKeyState(mapKeys[wParam], true); return 0;
        //        //    //case WM_KEYUP: ptrPGE->olc_UpdateKeyState(mapKeys[wParam], false); return 0;
        //    //case WM_SYSKEYDOWN: ptrPGE->olc_UpdateKeyState(mapKeys[wParam], true); return 0;
        //    //case WM_SYSKEYUP: ptrPGE->olc_UpdateKeyState(mapKeys[wParam], false); return 0;

        //    //case (uint)WindowsMessage.WM_CLOSE: Platform.PGE.olc_Terminate(); Console.WriteLine("terminate"); return IntPtr.Zero;
        //    //case (uint)WindowsMessage.WM_DESTROY: PostQuitMessage(0); DestroyWindow(hWnd); Console.WriteLine("destroy"); return IntPtr.Zero;
        //}
        //return DefWindowProc(hWnd, (WindowsMessage)msg, wParam, lParam);
        //}
    }
}
