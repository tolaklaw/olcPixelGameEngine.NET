using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using olc;

namespace Dear_ImGui_Sample
{
    /// <summary>
    /// A modified version of Veldrid.ImGui's ImGuiRenderer.
    /// Manages input for ImGui and handles rendering ImGui's DrawLists with Veldrid.
    /// </summary>
    public class ImGuiController : IDisposable
    {
        private bool _frameBegun;

        private int _vertexArray;
        private int _vertexBuffer;
        private int _vertexBufferSize;
        private int _indexBuffer;
        private int _indexBufferSize;

        private Texture _fontTexture;
        private Shader _shader;

        private int _windowWidth;
        private int _windowHeight;
        private PixelGameEngine pge;

        private System.Numerics.Vector2 _scaleFactor = System.Numerics.Vector2.One;

        /// <summary>
        /// Constructs a new ImGuiController.
        /// </summary>
        public ImGuiController(int width, int height, PixelGameEngine pge)
        {
            this.pge = pge;
            _windowWidth = width;
            _windowHeight = height;

            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);
            var io = ImGui.GetIO();
            io.Fonts.AddFontDefault();

            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

            CreateDeviceResources();
            SetKeyMappings();

            SetPerFrameImGuiData(1f / 60f);

            ImGui.NewFrame();
            _frameBegun = true;
        }

        public void WindowResized(int width, int height)
        {
            _windowWidth = width;
            _windowHeight = height;
        }

        public void DestroyDeviceObjects()
        {
            Dispose();
        }

        public void CreateDeviceResources()
        {
            Util.CreateVertexArray("ImGui", out _vertexArray);

            _vertexBufferSize = 10000;
            _indexBufferSize = 2000;

            Util.CreateVertexBuffer("ImGui", out _vertexBuffer);
            Util.CreateElementBuffer("ImGui", out _indexBuffer);
            GL.NamedBufferData(_vertexBuffer, _vertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.NamedBufferData(_indexBuffer, _indexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            RecreateFontDeviceTexture();

            string VertexSource = @"#version 330 core

uniform mat4 projection_matrix;

layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_texCoord;
layout(location = 2) in vec4 in_color;

out vec4 color;
out vec2 texCoord;

void main()
{
    gl_Position = projection_matrix * vec4(in_position, 0, 1);
    color = in_color;
    texCoord = in_texCoord;
}";
            string FragmentSource = @"#version 330 core

uniform sampler2D in_fontTexture;

in vec4 color;
in vec2 texCoord;

out vec4 outputColor;

void main()
{
    outputColor = color * texture(in_fontTexture, texCoord);
}";
            _shader = new Shader("ImGui", VertexSource, FragmentSource);

            GL.VertexArrayVertexBuffer(_vertexArray, 0, _vertexBuffer, IntPtr.Zero, Unsafe.SizeOf<ImDrawVert>());
            GL.VertexArrayElementBuffer(_vertexArray, _indexBuffer);

            GL.EnableVertexArrayAttrib(_vertexArray, 0);
            GL.VertexArrayAttribBinding(_vertexArray, 0, 0);
            GL.VertexArrayAttribFormat(_vertexArray, 0, 2, VertexAttribType.Float, false, 0);

            GL.EnableVertexArrayAttrib(_vertexArray, 1);
            GL.VertexArrayAttribBinding(_vertexArray, 1, 0);
            GL.VertexArrayAttribFormat(_vertexArray, 1, 2, VertexAttribType.Float, false, 8);

            GL.EnableVertexArrayAttrib(_vertexArray, 2);
            GL.VertexArrayAttribBinding(_vertexArray, 2, 0);
            GL.VertexArrayAttribFormat(_vertexArray, 2, 4, VertexAttribType.UnsignedByte, true, 16);

            Util.CheckGLError("End of ImGui setup");
        }

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        public void RecreateFontDeviceTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

            _fontTexture = new Texture("ImGui Text Atlas", width, height, pixels);
            _fontTexture.SetMagFilter(TextureMagFilter.Linear);
            _fontTexture.SetMinFilter(TextureMinFilter.Linear);

            io.Fonts.SetTexID((IntPtr)_fontTexture.GLTexture);

            io.Fonts.ClearTexData();
        }

        /// <summary>
        /// Renders the ImGui draw list data.
        /// This method requires a <see cref="GraphicsDevice"/> because it may create new DeviceBuffers if the size of vertex
        /// or index data has increased beyond the capacity of the existing buffers.
        /// A <see cref="CommandList"/> is needed to submit drawing and resource update commands.
        /// </summary>
        public void Render()
        {
            if (_frameBegun)
            {
                _frameBegun = false;
                ImGui.Render();
                RenderImDrawData(ImGui.GetDrawData());
            }
        }

        /// <summary>
        /// Updates ImGui input and IO configuration state.
        /// </summary>
        public void Update(GameWindow wnd, float deltaSeconds)
        {
            if (_frameBegun)
            {
                ImGui.Render();
            }

            SetPerFrameImGuiData(deltaSeconds);
            UpdateImGuiInput(wnd);

            _frameBegun = true;
            ImGui.NewFrame();
        }

        /// <summary>
        /// Sets per-frame data based on the associated window.
        /// This is called by Update(float).
        /// </summary>
        private void SetPerFrameImGuiData(float deltaSeconds)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(
                _windowWidth / _scaleFactor.X,
                _windowHeight / _scaleFactor.Y);
            io.DisplayFramebufferScale = _scaleFactor;
            io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
        }

        readonly List<char> PressedChars = new List<char>();

        private void UpdateImGuiInput(GameWindow wnd)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            //MouseState mouseState = new MouseState();
            

            io.MouseDown[0] = pge.GetMouse(0).bHeld;
            io.MouseDown[1] = pge.GetMouse(1).bHeld;
            io.MouseDown[2] = pge.GetMouse(2).bHeld;
            var screenPoint = new Vector2i(pge.GetMousePos().x, pge.GetMousePos().y);
            var point = screenPoint;
            io.MousePos = new System.Numerics.Vector2(point.X, point.Y);

            //KeyboardState KeyboardState = wnd.KeyboardState;


            foreach (olc.Key key in Enum.GetValues(typeof(olc.Key)))
            {
                if (key == Key.NONE) continue;
                
                io.KeysDown[(int)key] = pge.GetKey(key).bHeld;
                bool shiftState = pge.GetKey(Key.SHIFT).bHeld;
                if (pge.GetKey(key).bPressed)
                {

                    switch (key)
                    {
                        case Key.A:
                        case Key.B:
                        case Key.C:
                        case Key.D:
                        case Key.E:
                        case Key.F:
                        case Key.G:
                        case Key.H:
                        case Key.I:
                        case Key.J:
                        case Key.K:
                        case Key.L:
                        case Key.M:
                        case Key.N:
                        case Key.O:
                        case Key.P:
                        case Key.Q:
                        case Key.R:
                        case Key.S:
                        case Key.T:
                        case Key.U:
                        case Key.V:
                        case Key.W:
                        case Key.X:
                        case Key.Y:
                        case Key.Z:
                        case Key.K0:
                        case Key.K1:
                        case Key.K2:
                        case Key.K3:
                        case Key.K4:
                        case Key.K5:
                        case Key.K6:
                        case Key.K7:
                        case Key.K8:
                        case Key.K9:
                        case Key.EQUALS:
                        case Key.SPACE:
                        case Key.COMMA:
                        case Key.MINUS:
                        case Key.PERIOD:

                            io.AddInputCharacter(GetInputChar(key, shiftState));
                            break;
                        
                    }
                }
            }
                //io.AddInputCharacter(c);
            //}
                //{
                //    if (key == Keys.Unknown)
                //    {
                //        continue;
                //    }
                //    io.KeysDown[(int)key] = KeyboardState.IsKeyDown(key);
                //}

                //foreach (var c in PressedChars)
                //{
                //    io.AddInputCharacter(c);
                //}
              //  PressedChars.Clear();
            io.KeyCtrl = pge.GetKey(Key.CTRL).bHeld;
            io.KeyAlt = pge.GetKey(Key.OEM_1).bHeld | pge.GetKey(Key.OEM_2).bHeld; // no olc.key for this? 
            io.KeyShift = pge.GetKey(Key.SHIFT).bHeld;
            io.KeySuper = pge.GetKey(Key.OEM_3).bHeld; // no olc.key for this? windows key?
        }

        private char GetInputChar(Key key, bool shiftState)
        {
            string c;
            switch (key)
            {
                case Key.K0: c = "0";  break;
                case Key.K1: c = "1"; break;
                case Key.K2: c = "2"; break;
                case Key.K3: c = "3"; break;
                case Key.K4: c = "4"; break;
                case Key.K5: c = "5"; break;
                case Key.K6: c = "6"; break;
                case Key.K7: c = "7"; break;
                case Key.K8: c = "8"; break;
                case Key.K9: c = "9"; break;
                case Key.EQUALS: c = "="; break;
                case Key.SPACE: c = " "; break;
                case Key.COMMA: c = ","; break;
                case Key.MINUS: c = "-"; break;
                case Key.PERIOD: c = "."; break;
                default: c = key.ToString(); break;
            }
            
            if (!shiftState) c = c.ToLower();
            return c[0];
        }


        internal void PressChar(char keyChar)
        {
            PressedChars.Add(keyChar);
        }

        internal void MouseScroll(Vector2 offset)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            io.MouseWheel = offset.Y;
            io.MouseWheelH = offset.X;
        }

        private static void SetKeyMappings()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.KeyMap[(int)ImGuiKey.Tab] = (int)Key.TAB;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Key.LEFT;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Key.RIGHT;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Key.UP;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Key.DOWN;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)Key.PGUP;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)Key.PGDN;
            io.KeyMap[(int)ImGuiKey.Home] = (int)Key.HOME;
            io.KeyMap[(int)ImGuiKey.End] = (int)Key.END;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)Key.DEL;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)Key.BACK;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)Key.ENTER;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)Key.ESCAPE;
            io.KeyMap[(int)ImGuiKey.A] = (int)Key.A;
            io.KeyMap[(int)ImGuiKey.C] = (int)Key.C;
            io.KeyMap[(int)ImGuiKey.V] = (int)Key.V;
            io.KeyMap[(int)ImGuiKey.X] = (int)Key.X;
            io.KeyMap[(int)ImGuiKey.Y] = (int)Key.Y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)Key.Z;
        }

        

        private void RenderImDrawData(ImDrawDataPtr draw_data)
        {
            if (draw_data.CmdListsCount == 0)
            {
                return;
            }

            for (int i = 0; i < draw_data.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[i];

                int vertexSize = cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
                if (vertexSize > _vertexBufferSize)
                {
                    int newSize = (int)Math.Max(_vertexBufferSize * 1.5f, vertexSize);
                    GL.NamedBufferData(_vertexBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    _vertexBufferSize = newSize;

                    Console.WriteLine($"Resized dear imgui vertex buffer to new size {_vertexBufferSize}");
                }

                int indexSize = cmd_list.IdxBuffer.Size * sizeof(ushort);
                if (indexSize > _indexBufferSize)
                {
                    int newSize = (int)Math.Max(_indexBufferSize * 1.5f, indexSize);
                    GL.NamedBufferData(_indexBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    _indexBufferSize = newSize;

                    Console.WriteLine($"Resized dear imgui index buffer to new size {_indexBufferSize}");
                }
            }

            // Setup orthographic projection matrix into our constant buffer
            ImGuiIOPtr io = ImGui.GetIO();
            Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(
                0.0f,
                io.DisplaySize.X,
                io.DisplaySize.Y,
                0.0f,
                -1.0f,
                1.0f);

            _shader.UseShader();
            GL.UniformMatrix4(_shader.GetUniformLocation("projection_matrix"), false, ref mvp);
            GL.Uniform1(_shader.GetUniformLocation("in_fontTexture"), 0);
            Util.CheckGLError("Projection");

            GL.BindVertexArray(_vertexArray);
            Util.CheckGLError("VAO");

            draw_data.ScaleClipRects(io.DisplayFramebufferScale);

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ScissorTest);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);

            // Render command lists
            for (int n = 0; n < draw_data.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[n];

                GL.NamedBufferSubData(_vertexBuffer, IntPtr.Zero, cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmd_list.VtxBuffer.Data);
                Util.CheckGLError($"Data Vert {n}");

                GL.NamedBufferSubData(_indexBuffer, IntPtr.Zero, cmd_list.IdxBuffer.Size * sizeof(ushort), cmd_list.IdxBuffer.Data);
                Util.CheckGLError($"Data Idx {n}");

                int vtx_offset = 0;
                int idx_offset = 0;

                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);
                        Util.CheckGLError("Texture");

                        // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                        var clip = pcmd.ClipRect;
                        GL.Scissor((int)clip.X, _windowHeight - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));
                        Util.CheckGLError("Scissor");

                        if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                        {
                            GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(idx_offset * sizeof(ushort)), vtx_offset);
                        }
                        else
                        {
                            GL.DrawElements(BeginMode.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (int)pcmd.IdxOffset * sizeof(ushort));
                        }
                        Util.CheckGLError("Draw");
                    }

                    idx_offset += (int)pcmd.ElemCount;
                }
                vtx_offset += cmd_list.VtxBuffer.Size;
            }

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.ScissorTest);
        }

        /// <summary>
        /// Frees all graphics resources used by the renderer.
        /// </summary>
        public void Dispose()
        {
            _fontTexture.Dispose();
            _shader.Dispose();
        }
    }
}
