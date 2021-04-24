using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
//using OpenGL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.GraphicsLibraryFramework;
//using OpenTK.Platform.Windows;
//using OpenTK.Graphics;
//using OpenTK.Graphics.ES30;

namespace olc
{

    public unsafe class Renderer_OGL33 : Renderer
    {



        Window* window;
        const int OLC_MAX_VERTS = 128;
        bool bSync = false;
        DecalMode nDecalMode = DecalMode.ADDITIVE; // -1? 
        IntPtr glRenderContext;
        IntPtr glDeviceContext;
        int m_nFS = 0;
        int m_nVS = 0;
        int shaderProgram = 0;
        int m_vbQuad = 0;
        int m_vaQuad = 0;
        Renderable rendBlankQuad = new Renderable();
        string[] strVS;
        string[] strFS;
        List<string> vertexShaders = new List<string>();
        List<string> fragmentShaders = new List<string>();
   //     Window* window = null;
        string fragmentShaderSource = @"   
                                                #version 330 core
                                                out vec4 pixel;
                                                in vec2 oTex;                                                
                                                uniform sampler2D sprTex;
                                                
                                                void main()
                                                {
                                                    pixel = texture(sprTex, oTex);
                                                }
                ";
        /*
                 "#version 330 core\n"
             #endif
                             "out vec4 pixel;\n""in vec2 oTex;\n"
                             "in vec4 oCol;\n""uniform sampler2D sprTex;\n""void main(){pixel = texture(sprTex, oTex) * oCol;}";

              */


        string vertexShaderSource = @"
                                         #version 330 core
                                                layout (location = 0) in vec3 aPos;
                                                layout (location = 1) in vec2 aTex;
                                                
                                                out vec2 oTex;
                                                
                                                void main()
                                                {
                                                   gl_Position = vec4(aPos, 1.0);
                                                   oTex = aTex;
                                                   
                                                }
        ";

        /*
                   "layout(location = 0) in vec3 aPos;\n""layout(location = 1) in vec2 aTex;\n"
                   "layout(location = 2) in vec4 aCol;\n""out vec2 oTex;\n""out vec4 oCol;\n"
                   "void main(){ float p = 1.0 / aPos.z; gl_Position = p * vec4(aPos.x, aPos.y, 0.0, 1.0); oTex = p * aTex; oCol = aCol;}";
           */
        [StructLayout(LayoutKind.Explicit)]
        public struct Bvert
        {

            [FieldOffset(0)] public float x;
            [FieldOffset(4)] public float y;
            [FieldOffset(8)] public float z;
            [FieldOffset(12)] public float tx;
            [FieldOffset(16)] public float ty;
            [FieldOffset(20)] public byte r;
            [FieldOffset(21)] public byte g;
            [FieldOffset(22)] public byte b;
            [FieldOffset(23)] public byte a;

            public Bvert(float x, float y, float z, float tx, float ty, byte r, byte g, byte b, byte a)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.tx = tx;
                this.ty = ty;
                this.r = r;
                this.g = g;
                this.b = b;
                this.a = a;
            }
            public Bvert(float[] pos, vf2d text, Pixel col)
            {
                this.x = pos[0];
                this.y = pos[1];
                this.z = pos[2];
                this.tx = text.x;
                this.ty = text.y;
                this.r = col.r;
                this.g = col.g;
                this.b = col.b;
                this.a = col.a;
            }

            public Bvert(float[] pos, Pixel col)
            {
                this.x = pos[0];
                this.y = pos[1];
                this.z = pos[2];
                this.tx = pos[3];
                this.ty = pos[4];
                this.r = col.r;
                this.g = col.g;
                this.b = col.b;
                this.a = col.a;
            }

            public Bvert(float[] pos, vf2d text, byte r, byte g, byte b, byte a = 255)
            {
                this.x = pos[0];
                this.y = pos[1];
                this.z = pos[2];
                this.tx = text.x;
                this.ty = text.y;
                this.r = r;
                this.g = g;
                this.b = b;
                this.a = a;
            }
        }

        public Renderer_OGL33()
        {
            //Gl.Initialize();
        }

        public override void ApplyTexture(uint id)
        {
            while (GL.GetError() != OpenTK.Graphics.OpenGL.ErrorCode.NoError) ;
            GL.BindTexture(TextureTarget.Texture2D, id);
            //Gl.BindTexture(TextureTarget.Texture2d, id);
            var error = GL.GetError();
            if (error != OpenTK.Graphics.OpenGL.ErrorCode.NoError)
            {
                Console.WriteLine(error);
            }

        }

        public override void ClearBuffer(Pixel p, bool bDepth)
        {

            //GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            //GL.Clear(ClearBufferMask.ColorBufferBit);
            //Gl.ClearColor(1f, 1f, 1f, 1f);
            GL.ClearColor((float)(p.r) / 255.0f, (float)(p.g) / 255.0f, (float)(p.b) / 255.0f, (float)(p.a) / 255.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            //  if (bDepth) GL.Clear(ClearBufferMask.DepthBufferBit);

        }


        public override Const.rcode CreateDevice(List<IntPtr> parameters, bool bFullScreen, bool bVSYNC)
        {
            // Create OpenGL Context

            // Create Device Context
            //glDeviceContext = Platform_Helper.GetDC(parameters[0]);
            //OpenTK.Graphics.Wgl.Wgl.Arb. ChoosePixelFormat()
            //Wgl.PIXELFORMATDESCRIPTOR pfd = new Wgl.PIXELFORMATDESCRIPTOR()
            //{
            //    nSize = (short)sizeof(Wgl.PIXELFORMATDESCRIPTOR),
            //    dwFlags = Wgl.PixelFormatDescriptorFlags.DrawToWindow | Wgl.PixelFormatDescriptorFlags.SupportOpenGL | Wgl.PixelFormatDescriptorFlags.Doublebuffer,
            //    cColorBits = 32,
            //    iPixelType = Wgl.PixelFormatDescriptorPixelType.Rgba,
            //    iLayerType = Wgl.PixelFormatDescriptorLayerType.Main
            //};
            //int pf = Wgl.ChoosePixelFormat(glDeviceContext, ref pfd);
            //if (pf == 0) return Const.rcode.FAIL;
            //Wgl.SetPixelFormat(glDeviceContext, pf, ref pfd);            


               
            GLFW.MakeContextCurrent(window);
            
            GL.LoadBindings(new GLFWBindingsContext());
            //glRenderContext = Wgl.CreateContext(glDeviceContext);
            //if (glRenderContext == null) return Const.rcode.FAIL;
            //Wgl.MakeCurrent(glDeviceContext, glRenderContext);

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);

            string infoLog = "";
            infoLog = GL.GetShaderInfoLog(vertexShader);
            //GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int success);
            if (infoLog.Length > 0)
            {
                Console.WriteLine("ERROR::SHADER::VERTEX::COMPILATION_FAILED " + infoLog);
            }

            // FRAGMENT SHADER 
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);


            infoLog = GL.GetShaderInfoLog(fragmentShader);

            if (infoLog.Length > 0)
            {
                Console.WriteLine("ERROR::SHADER::FRAGMENT::COMPILATION_FAILED " + infoLog);
            }


            // SHADER PROGRAM 
            shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);
            infoLog = GL.GetProgramInfoLog(shaderProgram);
            if (infoLog.Length > 0)
            {

                Console.WriteLine("ERROR::PROGRAM::LINK_FAILED " + infoLog);
            }


          
            bSync = bVSYNC;
            if (!bSync) GLFW.SwapInterval(0);

            // Create Quad
            m_vbQuad = GL.GenBuffer();
            m_vaQuad = GL.GenVertexArray();
            GL.BindVertexArray(m_vaQuad);
            GL.BindBuffer(BufferTarget.ArrayBuffer, m_vbQuad);

            Bvert[] verts = new Bvert[OLC_MAX_VERTS];
            int vertsize = sizeof(Bvert);

            GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertsize, 0); GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, vertsize, (3 * sizeof(float))); GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            // Create blank texture for spriteless decals

            rendBlankQuad.Create(1, 1);
            rendBlankQuad.Sprite().GetData()[0] = Pixel.BLACK;
            rendBlankQuad.Decal().Update();

            return Const.rcode.OK;
        }

        public override uint CreateTexture(int width, int height, bool filtered = false, bool clamp = true)
        {
            // unused width, height

            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);


            if (filtered)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            }

            if (clamp)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            }
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, 0); //GL.MODULATE
            return (uint)id;


        }

        public override uint DeleteTexture(uint id)
        {
            GL.DeleteTexture(id);
            return id;
        }

        public override Const.rcode DestroyDevice()
        {
            
            //wgl.DeleteContext(glRenderContext);
            return Const.rcode.OK;
        }



        public override void DisplayFrame()
        {
            GLFW.SwapBuffers(window);
            //if (bSync) DwmFlush(); // Woooohooooooo!!!! SMOOOOOOOTH!
        }

        public override void DrawDecal(DecalInstance decal)
        {
            SetDecalMode(decal.mode);
            if (decal.decal == null)
                GL.BindTexture(TextureTarget.Texture2D, rendBlankQuad.Decal().id);
            else
                GL.BindTexture(TextureTarget.Texture2D, decal.decal.id);

            GL.BindBuffer(BufferTarget.ArrayBuffer, m_vbQuad);

            for (int i = 0; i < decal.points; i++)
            {
                //                pVertexMem[i] = new locVertex(new float[3] { decal.pos[i].x, decal.pos[i].y, decal.w[i] }, new vf2d(decal.uv[i].x, decal.uv[i].y), decal.tint[i]);
            }

            //          int vertexSize = Marshal.SizeOf(typeof(locVertex));

            //locBufferData(0x8892, sizeof(locVertex) * decal.points, pVertexMem, 0x88E0);
            //            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(vertexSize * decal.points), pVertexMem, BufferUsage.StreamDraw);

            if (nDecalMode == DecalMode.WIREFRAME)
                //glDrawArrays(GL_LINE_LOOP, 0, decal.points);
                GL.DrawArrays(PrimitiveType.LineLoop, 0, (int)decal.points);
            else
                GL.DrawArrays(PrimitiveType.TriangleFan, 0, (int)decal.points);

        }

        public override void DrawLayerQuad(vf2d offset, vf2d scale, Pixel tint)
        {

            GL.BindBuffer(BufferTarget.ArrayBuffer, m_vbQuad);
            const int numVerts = 4;
            const float voffset = 1f;
            Bvert[] verts =
                            new Bvert[numVerts] {    new Bvert(new float[5] { -voffset, -voffset, 1.0f ,    0.0f * scale.x + offset.x,      1.0f * scale.y + offset.y }, tint),
                                                     new Bvert(new float[5] { +voffset, -voffset, 1.0f ,    1.0f * scale.x + offset.x,      1.0f * scale.y + offset.y }, tint),
                                                     new Bvert(new float[5] { -voffset, +voffset, 1.0f ,    0.0f * scale.x + offset.x,      0.0f * scale.y + offset.y }, tint),
                                                     new Bvert(new float[5] { +voffset, +voffset, 1.0f ,    1.0f * scale.x + offset.x,      0.0f * scale.y + offset.y }, tint)
                            };
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(Bvert) * numVerts, verts, BufferUsageHint.StreamDraw);
            GL.BindTexture(TextureTarget.Texture2D, 3);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }

        public override void PrepareDevice() { }
        

        public override void PrepareDrawing()
        {
            GL.Enable(EnableCap.Blend);
            nDecalMode = DecalMode.NORMAL;
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.UseProgram(shaderProgram);
            GL.BindVertexArray(m_vaQuad);
        }

        public override void ReadTexture(uint id, Sprite spr)
        {
            GCHandle pinnedArray = GCHandle.Alloc(spr.GetData().ToArray(), GCHandleType.Pinned);
            IntPtr pointer = pinnedArray.AddrOfPinnedObject();

            GL.ReadPixels(0, 0, spr.width, spr.height, PixelFormat.Rgba, PixelType.UnsignedByte, pointer); ;
            pinnedArray.Free();

        }

        public override void SetDecalMode(DecalMode mode)
        {
            if (mode != nDecalMode)
            {
                switch (mode)
                {
                    case DecalMode.NORMAL:
                        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha); // glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
                        break;
                    case DecalMode.ADDITIVE:
                        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One); // glBlendFunc(GL_SRC_ALPHA, GL_ONE);
                        break;
                    case DecalMode.MULTIPLICATIVE:
                        GL.BlendFunc(BlendingFactor.DstColor, BlendingFactor.OneMinusSrcAlpha); //  //glBlendFunc(GL_DST_COLOR, GL_ONE_MINUS_SRC_ALPHA);
                        break;
                    case DecalMode.STENCIL:
                        GL.BlendFunc(BlendingFactor.Zero, BlendingFactor.SrcAlpha); //   //glBlendFunc(GL_ZERO, GL_SRC_ALPHA);
                        break;
                    case DecalMode.ILLUMINATE:
                        GL.BlendFunc(BlendingFactor.OneMinusSrcAlpha, BlendingFactor.SrcAlpha); // glBlendFunc(GL_ONE_MINUS_SRC_ALPHA, GL_SRC_ALPHA);
                        break;
                    case DecalMode.WIREFRAME:
                        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha); // glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
                        break;
                }
                nDecalMode = mode;
            }
        }
        
        public override void UpdateTexture(uint id, Sprite spr)
        {         
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, spr.width, spr.height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, spr.GetData().ToArray());
        }

        public override void UpdateViewport(vi2d pos, vi2d size)
        {
            GL.Viewport(pos.x, pos.y, size.x, size.y);
        }

        public override unsafe void SetWindowObject(Window* window)
        {
            this.window = window;
        }
    }
}
