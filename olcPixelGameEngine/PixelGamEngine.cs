#define OLC_GFX_OPENGL33
using System;
using System.Collections.Generic;
using System.Threading;
using static olc.Const;
using static olc.Pixel;
using static olc.Sprite;

namespace olc
{
  
    public class PixelGameEngine
    {
        private Clock clock;
        public PixelGameEngine()
        {
            sAppName = "Undefined";
            //pge = this;

            // Bring in relevant Platform &Rendering systems depending on compiler parameters
            olc_ConfigureSystem();
            clock = new Clock();
            m_tp1 = clock.UtcNow;
        }

        public rcode Construct(int screen_w, int screen_h, int pixel_w, int pixel_h,bool full_screen = false, bool vsync = false, bool cohesion = false)
        {
            bPixelCohesion = cohesion;
            vScreenSize = new vi2d(screen_w, screen_h);
            vInvScreenSize = new vf2d(1.0f / (float)screen_w, 1.0f / (float)screen_h);
            vPixelSize = new vi2d(pixel_w, pixel_h);
            vWindowSize = vScreenSize * vPixelSize;
            bFullScreen = full_screen;
            bEnableVSYNC = vsync;
            vPixel = vScreenSize / 2.0f;

            if (vPixelSize.x <= 0 || vPixelSize.y <= 0 || vScreenSize.x <= 0 || vScreenSize.y <= 0)
                return rcode.FAIL;
            return rcode.OK;
        }
        public rcode Start()
        {
            if (platform.ApplicationStartUp() != rcode.OK) return rcode.FAIL;

            // Construct the window
            if (platform.CreateWindowPane(new vi2d(30, 30), vWindowSize, bFullScreen) != rcode.OK) return rcode.FAIL;
            olc_UpdateWindowSize(vWindowSize.x, vWindowSize.y);

            // Start the thread
            bAtomActive = true;
            Thread t = new Thread(new ThreadStart(EngineThread));
            t.Start();
            // Some implementations may form an event loop here
            
            platform.StartSystemEventLoop();
            //EngineThread();
            // Wait for thread to be exited
            t.Join();

            if (platform.ApplicationCleanUp() != rcode.OK) return rcode.FAIL;

            return rcode.OK;
        }

        // User Override Interfaces
        // Called once on application startup, use to load your resources
        public virtual bool OnUserCreate() => false;

        // Called every frame, and provides you with a time per frame value
        public virtual bool OnUserUpdate(float fElapsedTime) => false;

        // Called once on application termination, so you can be one clean coder
        public virtual bool OnUserDestroy() => true;

        // Hardware Interfaces
        // Returns true if window is currently in focus
        public bool IsFocused() => bHasInputFocus;
        // Get the state of a specific keyboard buttonuint32
        public HWButton GetKey(Key k) => pKeyboardState[(int)k];
        // Get the state of a specific mouse button
        public HWButton GetMouse(UInt32 b) => pMouseState[b];
        // Get Mouse X coordinate in "pixel" space
        public int GetMouseX() => vMousePos.x;
        // Get Mouse Y coordinate in "pixel" space
        public int GetMouseY() => vMousePos.y;
        // Get Mouse Wheel Delta
        public int GetMouseWheel() => nMouseWheelDelta;
        // Get the mouse in window space
        public vi2d GetWindowMouse() => vMouseWindowPos;
        // Gets the mouse as a vector to keep Tarriest happy
        public vi2d GetMousePos() => vMousePos;

        // Utility
        // Returns the width of the screen in "pixels"
        public int ScreenWidth() => vScreenSize.x;
        // Returns the height of the screen in "pixels"
        public int ScreenHeight() => vScreenSize.y;
        // Returns the width of the currently selected drawing target in "pixels"
        public int GetDrawTargetWidth() => (pDrawTarget == null) ? 0 : pDrawTarget.width;
        // Returns the height of the currently selected drawing target in "pixels"
        public int GetDrawTargetHeight() => (pDrawTarget == null) ? 0 : pDrawTarget.height;
        // Returns the currently active draw target
        public Sprite GetDrawTarget() => pDrawTarget;
        // Resize the primary screen sprite
        public void SetScreenSize(int w, int h)
        {
            vScreenSize = new vi2d(w, h);
            vInvScreenSize = new vf2d(1.0f / (float)w, 1.0f / (float)h);
            foreach (var layer in vLayers)
            {                
                layer.pDrawTarget = new Sprite(vScreenSize.x, vScreenSize.y);
                layer.bUpdate = true;
            }
            SetDrawTarget(null);
            renderer.ClearBuffer(BLACK, true);
            renderer.DisplayFrame();
            renderer.ClearBuffer(BLACK, true);
            renderer.UpdateViewport(vViewPos, vViewSize);
            Console.WriteLine("SetScreenSize()");
        }
        // Specify which Sprite should be the target of drawing functions, use nullptr
        // to specify the primary screen
        public void SetDrawTarget(Sprite target)
        {
            if (target == null)
            {
                nTargetLayer = 0;
                pDrawTarget = vLayers[0].pDrawTarget;
            } else
            {
                pDrawTarget = target;
            }
            
        }
        // Gets the current Frames Per Second
        public int GetFPS() => nLastFPS;
        // Gets last update of elapsed time
        public float GetElapsedTime() => fLastElapsed;
        // Gets Actual Window size
        public vi2d GetWindowSize() => vWindowSize;
        // Gets pixel scale
        public vi2d GetPixelSize() => vPixelSize;
        // Gets actual pixel scale
        public vi2d GetScreenPixelSize() => vScreenPixelSize;

        // CONFIGURATION ROUTINES
        // Layer targeting functions
        public void SetDrawTarget(byte layer)
        {
            if (layer < vLayers.Count)
            {
                pDrawTarget = vLayers[layer].pDrawTarget;
                vLayers[layer].bUpdate = true;
                nTargetLayer = layer;
            }
        }
        public void EnableLayer(byte layer, bool b)
        {
            if (layer < vLayers.Count) vLayers[layer].bShow = b;
        }
        public void SetLayerOffset(byte layer, vf2d offset) => SetLayerOffset(layer, offset.x, offset.y);
        public void SetLayerOffset(byte layer, float x, float y)
        {
            if (layer < vLayers.Count) vLayers[layer].vOffset = new vf2d(x, y);
        }
        public void SetLayerScale(byte layer, vf2d scale) => SetLayerScale(layer, scale.x, scale.y);
        public void SetLayerScale(byte layer, float x, float y)
        {
            if (layer < vLayers.Count) vLayers[layer].vScale = new vf2d(x, y);
        }
        public void SetLayerTint(byte layer, Pixel tint)
        {
            if (layer < vLayers.Count) vLayers[layer].tint = tint;
        }
        public void SetLayerCustomRenderFunction(byte layer, Action function)
        {
            if (layer < vLayers.Count) vLayers[layer].funcHook = function;
        }

        public List<LayerDesc> GetLayers() => vLayers;
        public int CreateLayer()
        {
            LayerDesc ld = new LayerDesc();
            ld.pDrawTarget = new Sprite(vScreenSize.x, vScreenSize.y);
            ld.nResID = renderer.CreateTexture(vScreenSize.x, vScreenSize.y);
            renderer.UpdateTexture(ld.nResID, ld.pDrawTarget);
            vLayers.Add(ld);
            return vLayers.Count - 1;
        }

        // Change the pixel mode for different optimisations
        // Pixel::NORMAL = No transparency
        // Pixel::MASK   = Transparent if alpha is < 255
        // Pixel::ALPHA  = Full transparency
        public void SetPixelMode(PixelMode m) => nPixelMode = m;

        public PixelMode GetPixelMode() => nPixelMode;

        // Use a custom blend function
        public void SetPixelMode(Func<int, int, Pixel, Pixel, Pixel> pixelMode)
        {
            funcPixelMode = pixelMode;
            nPixelMode = PixelMode.CUSTOM;
        }

        // Change the blend factor from between 0.0f to 1.0f;
        public void SetPixelBlend(float fBlend)
        {
            fBlendFactor = fBlend;
            if (fBlendFactor < 0.0f) fBlendFactor = 0.0f;
            if (fBlendFactor > 1.0f) fBlendFactor = 1.0f;
        }




        // DRAWING ROUTINES
        // Draws a single Pixel
        // This is it, the critical function that plots a pixel
        public bool Draw(vi2d pos, Pixel p) => Draw(pos.x, pos.y, p);
        public virtual bool Draw(int x, int y, Pixel p)
        {
            if (pDrawTarget == null) return false;

            if (nPixelMode == PixelMode.NORMAL)
            {
                return pDrawTarget.SetPixel(x, y, p);
            }

            if (nPixelMode == PixelMode.MASK)
            {
                if (p.a == 255)
                    return pDrawTarget.SetPixel(x, y, p);
            }

            if (nPixelMode == PixelMode.ALPHA)
            {
                Pixel d = pDrawTarget.GetPixel(x, y);
                float a = (float)(p.a / 255.0f) * fBlendFactor;
                float c = 1.0f - a;
                float r = a * (float)p.r + c * (float)d.r;
                float g = a * (float)p.g + c * (float)d.g;
                float b = a * (float)p.b + c * (float)d.b;
                return pDrawTarget.SetPixel(x, y, new Pixel((byte)(r*255), (byte)(g*255), (byte)(b*255) /*, (uint8_t)(p.a * fBlendFactor)*/));
            }

            if (nPixelMode == PixelMode.CUSTOM)
            {
                return pDrawTarget.SetPixel(x, y, funcPixelMode(x, y, p, pDrawTarget.GetPixel(x, y)));
            }

            return false;
        }
        // Draws a line from (x1,y1) to (x2,y2)
        public void DrawLine(vi2d pos1, vi2d pos2, Pixel p, uint pattern = 0xFFFFFFFF) => DrawLine(pos1.x, pos1.y, pos2.x, pos2.y, p, pattern);
        public void DrawLine(int x1, int y1, int x2, int y2, Pixel p, uint pattern = 0xFFFFFFFF)
        {
            int x, y, dx, dy, dx1, dy1, px, py, xe, ye, i;
            dx = x2 - x1; dy = y2 - y1;
            int tmp;
            bool rol() { pattern = (pattern << 1) | (pattern >> 31); return (pattern & 1) == 1; }
            // straight lines idea by gurkanctn
            if (dx == 0) // Line is vertical
            {
                if (y2 < y1) { tmp = y1; y1 = y2; y2 = tmp; }
                for (y = y1; y <= y2; y++) if (rol()) Draw(x1, y, p);
                return;
            }

            if (dy == 0) // Line is horizontal
            {
                if (x2 < x1) { tmp = x1; x1 = x2; x2 = tmp; }
                for (x = x1; x <= x2; x++) if (rol()) Draw(x, y1, p);
                return;
            }

            // Line is Funk-aye
            dx1 = Math.Abs(dx); dy1 = Math.Abs(dy);
            px = 2 * dy1 - dx1; py = 2 * dx1 - dy1;
            if (dy1 <= dx1)
            {
                if (dx >= 0)
                {
                    x = x1; y = y1; xe = x2;
                }
                else
                {
                    x = x2; y = y2; xe = x1;
                }

                if (rol()) Draw(x, y, p);

                for (i = 0; x < xe; i++)
                {
                    x = x + 1;
                    if (px < 0)
                        px = px + 2 * dy1;
                    else
                    {
                        if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) y++; else y--;
                        px = px + 2 * (dy1 - dx1);
                    }
                    if (rol()) Draw(x, y, p);
                }
            }
            else
            {
                if (dy >= 0)
                {
                    x = x1; y = y1; ye = y2;
                }
                else
                {
                    x = x2; y = y2; ye = y1;
                }

                if (rol()) Draw(x, y, p);

                for (i = 0; y < ye; i++)
                {
                    y = y + 1;
                    if (py <= 0)
                        py = py + 2 * dx1;
                    else
                    {
                        if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) x = x + 1; else x = x - 1;
                        py = py + 2 * (dx1 - dy1);
                    }
                    if (rol()) Draw(x, y, p);
                }
            }
        }

        // Draws a circle located at (x,y) with radius
        public void DrawCircle(vi2d pos, int radius, Pixel p, byte mask = 0xFF) => DrawCircle(pos.x, pos.y, radius, p, mask);
        public void DrawCircle(int x, int y, int radius, Pixel p, byte mask = 0xFF)
        { // Thanks to IanM-Matrix1 #PR121
            if (radius < 0 || x < -radius || y < -radius || x - GetDrawTargetWidth() > radius || y - GetDrawTargetHeight() > radius)
                return;

            if (radius > 0)
            {
                int x0 = 0;
                int y0 = radius;
                int d = 3 - 2 * radius;

                while (y0 >= x0) // only formulate 1/8 of circle
                {
                    // Draw even octants
                    if ((mask & 0x01) != 0) Draw(x + x0, y - y0, p);// Q6 - upper right right
                    if ((mask & 0x04) != 0) Draw(x + y0, y + x0, p);// Q4 - lower lower right
                    if ((mask & 0x10) != 0) Draw(x - x0, y + y0, p);// Q2 - lower left left
                    if ((mask & 0x40) != 0) Draw(x - y0, y - x0, p);// Q0 - upper upper left
                    if (x0 != 0 && x0 != y0)
                    {
                        if ((mask & 0x02) !=0) Draw(x + y0, y - x0, p);// Q7 - upper upper right
                        if ((mask & 0x08) != 0) Draw(x + x0, y + y0, p);// Q5 - lower right right
                        if ((mask & 0x20) !=0) Draw(x - y0, y + x0, p);// Q3 - lower lower left
                        if ((mask & 0x80) !=0) Draw(x - x0, y - y0, p);// Q1 - upper left left
                    }

                    if (d < 0)
                        d += 4 * x0++ + 6;
                    else
                        d += 4 * (x0++ - y0--) + 10;
                }
            }
            else
                Draw(x, y, p);
        }

        // Fills a circle located at (x,y) with radius
        public void FillCircle(vi2d pos, Int32 radius, Pixel p) => FillCircle(pos.x, pos.y, radius, p);
        public void FillCircle(Int32 x, Int32 y, Int32 radius, Pixel p)
        {
            // Thanks to IanM-Matrix1 #PR121
            if (radius < 0 || x < -radius || y < -radius || x - GetDrawTargetWidth() > radius || y - GetDrawTargetHeight() > radius)
                return;

            if (radius > 0)
            {
                int x0 = 0;
                int y0 = radius;
                int d = 3 - 2 * radius;
                void qdrawline(int sx, int ex, int y) { for (int x = sx; x <= ex; x++) Draw(x, y, p); }
                while (y0 >= x0)
                {
                    qdrawline(x - y0, x + y0, y - x0);
                    if (x0 > 0) qdrawline(x - y0, x + y0, y + x0);

                    if (d < 0)

                        d += 4 * x0++ + 6;
                    else
                    {
                        if (x0 != y0)
                        {
                            qdrawline(x - x0, x + x0, y - y0);
                            qdrawline(x - x0, x + x0, y + y0);
                        }
                        d += 4 * (x0++ - y0--) + 10;
                    }
                }
            }
            else
                Draw(x, y, p);
        }

        // Draws a rectangle at (x,y) to (x+w,y+h)
        public void DrawRect(vi2d pos, vi2d size, Pixel p) => DrawRect(pos.x, pos.y, size.x, size.y, p);
        public void DrawRect(Int32 x, Int32 y, Int32 w, Int32 h, Pixel p)
        {
            DrawLine(x, y, x + w, y, p);
            DrawLine(x + w, y, x + w, y + h, p);
            DrawLine(x + w, y + h, x, y + h, p);
            DrawLine(x, y + h, x, y, p);
        }
        // Fills a rectangle at (x,y) to (x+w,y+h)
        public void FillRect(vi2d pos, vi2d size, Pixel p) => FillRect(pos.x, pos.y, size.x, size.y, p);
        public void FillRect(Int32 x, Int32 y, Int32 w, Int32 h, Pixel p)
        {
            int x2 = x + w;
            int y2 = y + h;

            if (x < 0) x = 0;
            if (x >= (int)GetDrawTargetWidth()) x = (int)GetDrawTargetWidth();
            if (y < 0) y = 0;
            if (y >= (int)GetDrawTargetHeight()) y = (int)GetDrawTargetHeight();

            if (x2 < 0) x2 = 0;
            if (x2 >= (int)GetDrawTargetWidth()) x2 = (int)GetDrawTargetWidth();
            if (y2 < 0) y2 = 0;
            if (y2 >= (int)GetDrawTargetHeight()) y2 = (int)GetDrawTargetHeight();

            for (int i = x; i < x2; i++)
                for (int j = y; j < y2; j++)
                    Draw(i, j, p);
        }

        // Draws a triangle between points (x1,y1), (x2,y2) and (x3,y3)
        public void DrawTriangle(vi2d pos1, vi2d pos2, vi2d pos3, Pixel p) => DrawTriangle(pos1.x, pos1.y, pos2.x, pos2.y, pos3.x, pos3.y, p);
        public void DrawTriangle(Int32 x1, Int32 y1, Int32 x2, Int32 y2, Int32 x3, Int32 y3, Pixel p)
        {
            DrawLine(x1, y1, x2, y2, p);
            DrawLine(x2, y2, x3, y3, p);
            DrawLine(x3, y3, x1, y1, p);
        }

        // Flat fills a triangle between points (x1,y1), (x2,y2) and (x3,y3)
        public void FillTriangle(vi2d pos1, vi2d pos2, vi2d pos3, Pixel p) => FillTriangle(pos1.x, pos1.y, pos2.x, pos2.y, pos3.x, pos3.y, p);
        public void FillTriangle(Int32 x1, Int32 y1, Int32 x2, Int32 y2, Int32 x3, Int32 y3, Pixel p)
        {
            void qdrawline(int sx, int ex, int ny) { for (int i = sx; i <= ex; i++) Draw(i, ny, p); }
            void swap(ref int a, ref int b)
            {
                int c = a;
                a = b;
                b = c;
            }
            //auto drawline = [&](int sx, int ex, int ny) { for (int i = sx; i <= ex; i++) Draw(i, ny, p); };

            int t1x, t2x, y, minx, maxx, t1xp, t2xp;
            bool changed1 = false;
            bool changed2 = false;
            int signx1, signx2, dx1, dy1, dx2, dy2;
            int e1, e2;
            // Sort vertices
            if (y1 > y2) { swap(ref y1, ref y2); swap(ref x1, ref x2); }
            if (y1 > y3) { swap(ref y1, ref y3); swap(ref x1, ref x3); }
            if (y2 > y3) { swap(ref y2, ref y3); swap(ref x2, ref x3); }

            t1x = t2x = x1; y = y1;   // Starting points
            dx1 = (int)(x2 - x1);
            if (dx1 < 0) { dx1 = -dx1; signx1 = -1; }
            else signx1 = 1;
            dy1 = (int)(y2 - y1);

            dx2 = (int)(x3 - x1);
            if (dx2 < 0) { dx2 = -dx2; signx2 = -1; }
            else signx2 = 1;
            dy2 = (int)(y3 - y1);

            if (dy1 > dx1) { swap(ref dx1, ref dy1); changed1 = true; }
            if (dy2 > dx2) { swap(ref dy2, ref dx2); changed2 = true; }

            e2 = (int)(dx2 >> 1);
            // Flat top, just process the second half
            if (y1 == y2) goto next;
            e1 = (int)(dx1 >> 1);

            for (int i = 0; i < dx1;)
            {
                t1xp = 0; t2xp = 0;
                if (t1x < t2x) { minx = t1x; maxx = t2x; }
                else { minx = t2x; maxx = t1x; }
                // process first line until y value is about to change
                while (i < dx1)
                {
                    i++;
                    e1 += dy1;
                    while (e1 >= dx1)
                    {
                        e1 -= dx1;
                        if (changed1) t1xp = signx1;//t1x += signx1;
                        else goto next1;
                    }
                    if (changed1) break;
                    else t1x += signx1;
                }
            // Move line
            next1:
                // process second line until y value is about to change
                while (true)
                {
                    e2 += dy2;
                    while (e2 >= dx2)
                    {
                        e2 -= dx2;
                        if (changed2) t2xp = signx2;//t2x += signx2;
                        else goto next2;
                    }
                    if (changed2) break;
                    else t2x += signx2;
                }
            next2:
                if (minx > t1x) minx = t1x;
                if (minx > t2x) minx = t2x;
                if (maxx < t1x) maxx = t1x;
                if (maxx < t2x) maxx = t2x;
                qdrawline(minx, maxx, y);    // Draw line from min to max points found on the y
                                             // Now increase y
                if (!changed1) t1x += signx1;
                t1x += t1xp;
                if (!changed2) t2x += signx2;
                t2x += t2xp;
                y += 1;
                if (y == y2) break;
            }
        next:
            // Second half
            dx1 = (int)(x3 - x2); if (dx1 < 0) { dx1 = -dx1; signx1 = -1; }
            else signx1 = 1;
            dy1 = (int)(y3 - y2);
            t1x = x2;

            if (dy1 > dx1)
            {   // swap values
                swap(ref dy1, ref dx1);
                changed1 = true;
            }
            else changed1 = false;

            e1 = (int)(dx1 >> 1);

            for (int i = 0; i <= dx1; i++)
            {
                t1xp = 0; t2xp = 0;
                if (t1x < t2x) { minx = t1x; maxx = t2x; }
                else { minx = t2x; maxx = t1x; }
                // process first line until y value is about to change
                while (i < dx1)
                {
                    e1 += dy1;
                    while (e1 >= dx1)
                    {
                        e1 -= dx1;
                        if (changed1) { t1xp = signx1; break; }//t1x += signx1;
                        else goto next3;
                    }
                    if (changed1) break;
                    else t1x += signx1;
                    if (i < dx1) i++;
                }
            next3:
                // process second line until y value is about to change
                while (t2x != x3)
                {
                    e2 += dy2;
                    while (e2 >= dx2)
                    {
                        e2 -= dx2;
                        if (changed2) t2xp = signx2;
                        else goto next4;
                    }
                    if (changed2) break;
                    else t2x += signx2;
                }
            next4:

                if (minx > t1x) minx = t1x;
                if (minx > t2x) minx = t2x;
                if (maxx < t1x) maxx = t1x;
                if (maxx < t2x) maxx = t2x;
                qdrawline(minx, maxx, y);
                if (!changed1) t1x += signx1;
                t1x += t1xp;
                if (!changed2) t2x += signx2;
                t2x += t2xp;
                y += 1;
                if (y > y3) return;
            }
        }

        // Draws an entire sprite at location (x,y)
        public void DrawSprite(vi2d pos, Sprite sprite, uint scale = 1, Flip flip = Flip.NONE) => DrawSprite(pos.x, pos.y, sprite, scale, flip);
        public void DrawSprite(int x, int y, Sprite sprite, uint scale = 1, Flip flip = Flip.NONE)
        {
            if (sprite == null)
                return;

            int fxs = 0, fxm = 1, fx = 0;
            int fys = 0, fym = 1, fy = 0;
            if (flip == Flip.HORIZ) { fxs = (int)sprite.width - 1; fxm = -1; }
            if (flip == Flip.VERT) { fys = (int)sprite.height - 1; fym = -1; }

            if (scale > 1)
            {
                fx = fxs;
                for (int i = 0; i < sprite.width; i++, fx += fxm)
                {
                    fy = fys;
                    for (int j = 0; j < sprite.height; j++, fy += fym)
                        for (int _is = 0; _is < scale; _is++)
                            for (uint js = 0; js < scale; js++)
                                Draw((int)(x + (i * scale) + _is), (int)(y + (j * scale) + js), sprite.GetPixel(fx, fy));
                }
            }
            else
            {
                fx = fxs;
                for (int i = 0; i < sprite.width; i++, fx += fxm)
                {
                    fy = fys;
                    for (int j = 0; j < sprite.height; j++, fy += fym)
                        Draw(x + i, y + j, sprite.GetPixel(fx, fy));
                }
            }
        }
        // Draws an area of a sprite at location (x,y), where the
        // selected area is (ox,oy) to (ox+w,oy+h)
        public void DrawPartialSprite(vi2d pos, Sprite sprite, vi2d sourcepos, vi2d size, uint scale = 1, Flip flip = Flip.NONE) => DrawPartialSprite(pos.x, pos.y, sprite, sourcepos.x, sourcepos.y, size.x, size.y, scale, flip);
        public void DrawPartialSprite(int x, int y, Sprite sprite, int ox, int oy, int w, int h, uint scale = 1, Flip flip = Flip.NONE)
        {
            if (sprite == null)
                return;

            int fxs = 0, fxm = 1, fx = 0;
            int fys = 0, fym = 1, fy = 0;
            if (flip == Flip.HORIZ) { fxs = w - 1; fxm = -1; }
            if (flip == Flip.VERT) { fys = h - 1; fym = -1; }

            if (scale > 1)
            {
                fx = fxs;
                for (int i = 0; i < w; i++, fx += fxm)
                {
                    fy = fys;
                    for (int j = 0; j < h; j++, fy += fym)
                        for (int _is = 0; _is < scale; _is++)
                            for (int js = 0; js < scale; js++)
                                Draw((int)(x + (i * scale) + _is), (int)(y + (j * scale) + js), sprite.GetPixel(fx + ox, fy + oy));
                }
            }
            else
            {
                fx = fxs;
                for (int i = 0; i < w; i++, fx += fxm)
                {
                    fy = fys;
                    for (int j = 0; j < h; j++, fy += fym)
                        Draw(x + i, y + j, sprite.GetPixel(fx + ox, fy + oy));
                }
            }
        }

        // Draws a single line of text - traditional monospaced
        public void DrawString(vi2d pos, string sText, Pixel col, int scale = 1) => DrawString(pos.x, pos.y, sText, col, scale);
        public void DrawString(int x, int y, string sText, Pixel col, int scale = 1)
        {
            int sx = 0;
            int sy = 0;
            PixelMode m = nPixelMode;
            // Thanks @tucna, spotted bug with col.ALPHA :P
            if (m != PixelMode.CUSTOM) // Thanks @Megarev, required for "shaders"
            {
                if (col.a != 255) SetPixelMode(PixelMode.ALPHA);
                else SetPixelMode(PixelMode.MASK);
            }
            foreach (var c in sText)
            {
                if (c == '\n')
                {
                    sx = 0; sy += 8 * scale;
                }
                else
                {
                    int ox = (c - 32) % 16;
                    int oy = (c - 32) / 16;

                    if (scale > 1)
                    {
                        for (int i = 0; i < 8; i++)
                            for (int j = 0; j < 8; j++)
                                if (fontSprite.GetPixel(i + ox * 8, j + oy * 8).r > 0)
                                    for (int _is = 0; _is < scale; _is++)
                                        for (int js = 0; js < scale; js++)
                                            Draw(x + sx + (i * scale) + _is, y + sy + (j * scale) + js, col);
                    }
                    else
                    {
                        for (int i = 0; i < 8; i++)
                            for (int j = 0; j < 8; j++)
                                if (fontSprite.GetPixel(i + ox * 8, j + oy * 8).r > 0)
                                    Draw(x + sx + i, y + sy + j, col);
                    }
                    sx += 8 * scale;
                }
            }
            SetPixelMode(m);
        }


        public vi2d GetTextSize(string s)
        {
            vi2d size = new vi2d(0, 1);
            vi2d pos = new vi2d(0, 1);
            foreach (var c in s)
            {
                if (c == '\n') { pos.y++; pos.x = 0; }
                else pos.x++;
                size.x = Math.Max(size.x, pos.x);
                size.y = Math.Max(size.y, pos.y);
            }
            return size * 8;
        }
        // Draws a single line of text - non-monospaced
        public void DrawStringProp(vi2d pos, string sText, Pixel col, UInt32 scale = 1) => DrawStringProp(pos.x, pos.y, sText, col, scale);
        public void DrawStringProp(Int32 x, Int32 y, string sText, Pixel col, UInt32 scale = 1)
        {
            int sx = 0;
            int sy = 0;
            PixelMode m = nPixelMode;

            if (m != PixelMode.CUSTOM)
            {
                if (col.a != 255) SetPixelMode(PixelMode.ALPHA);
                else SetPixelMode(PixelMode.MASK);
            }
            foreach (var c in sText)
            {
                if (c == '\n')
                {
                    sx = 0; sy += (int)(8 * scale);
                }
                else
                {
                    int ox = (c - 32) % 16;
                    int oy = (c - 32) / 16;

                    if (scale > 1)
                    {
                        for (int i = 0; i < vFontSpacing[c - 32].y; i++)
                            for (int j = 0; j < 8; j++)
                                if (fontSprite.GetPixel(i + ox * 8 + vFontSpacing[c - 32].x, j + oy * 8).r > 0)
                                    for (int _is = 0; _is < scale; _is++)
                                        for (int js = 0; js < scale; js++)
                                            Draw((int)(x + sx + (i * scale) + _is), (int)(y + sy + (j * scale) + js), col);
                    }
                    else
                    {
                        for (int i = 0; i < vFontSpacing[c - 32].y; i++)
                            for (int j = 0; j < 8; j++)
                                if (fontSprite.GetPixel(i + ox * 8 + vFontSpacing[c - 32].x, j + oy * 8).r > 0)
                                    Draw(x + sx + i, y + sy + j, col);
                    }
                    sx += (int)(vFontSpacing[c - 32].y * scale);
                }
            }
            SetPixelMode(m);
        }

        public vi2d GetTextSizeProp(string s)
        {
            vi2d size = new vi2d(0, 1);
            vi2d pos = new vi2d(0, 1);
            foreach (var c in s)
            {
                if (c == '\n') { pos.y += 1; pos.x = 0; }
                else pos.x += vFontSpacing[c - 32].y;
                size.x = Math.Max(size.x, pos.x);
                size.y = Math.Max(size.y, pos.y);
            }

            size.y *= 8;
            return size;
        }

        // Decal Quad functions
        public void SetDecalMode(DecalMode mode) => nDecalMode = mode;
        // Draws a whole decal, with optional scale and tinting
        public void DrawDecal(vf2d pos, Decal decal, vf2d scale, Pixel tint)
        {
            vf2d vScreenSpacePos = new olc.vf2d((float)((Math.Floor(pos.x) * vInvScreenSize.x) * 2.0f - 1.0f), (float)(((Math.Floor(pos.y) * vInvScreenSize.y) * 2.0f - 1.0f) * -1.0f));

            vf2d vScreenSpaceDim = new vf2d((int)(vScreenSpacePos.x + (2.0f * (decal.sprite.width) * vInvScreenSize.x) * scale.x), (int)(vScreenSpacePos.y - (2.0f * (decal.sprite.height) * vInvScreenSize.y) * scale.y));
            DecalInstance di = new DecalInstance();
            di.decal = decal;
            di.points = 4;
            di.tint = new List<Pixel>() { tint, tint, tint, tint };
            di.pos = new List<vf2d>() { new vf2d(vScreenSpacePos.x, vScreenSpacePos.y), new vf2d(vScreenSpacePos.x, vScreenSpaceDim.y), new vf2d(vScreenSpaceDim.x, vScreenSpaceDim.y), new vf2d(vScreenSpaceDim.x, vScreenSpacePos.y) };
            di.uv = new List<vf2d>() { new vf2d(0.0f, 0.0f), new vf2d(0.0f, 1.0f), new vf2d(1.0f, 1.0f), new vf2d(1.0f, 0.0f) };
            di.w = new List<float>() { 1, 1, 1, 1 };
            di.mode = nDecalMode;
            vLayers[nTargetLayer].vecDecalInstance.Add(di);
        }
        // Draws a region of a decal, with optional scale and tinting
        void DrawPartialDecal(vf2d pos, Decal decal, vf2d source_pos, vf2d source_size, vf2d scale, Pixel tint)
        {
            vf2d vScreenSpacePos = new vf2d(
                (float)((Math.Floor(pos.x) * vInvScreenSize.x) * 2.0f - 1.0f),
                (float)(((Math.Floor(pos.y) * vInvScreenSize.y) * 2.0f - 1.0f) * -1.0f)
            );

            vf2d vScreenSpaceDim = new vf2d(
                vScreenSpacePos.x + (2.0f * source_size.x * vInvScreenSize.x) * scale.x,
                (vScreenSpacePos.y - (2.0f * source_size.y * vInvScreenSize.y) * scale.y)
            );

            DecalInstance di;
            di.points = 4;
            di.decal = decal;
            di.tint = new List<Pixel>() { tint, tint, tint, tint };
            di.pos = new List<vf2d>() { new vf2d(vScreenSpacePos.x, vScreenSpacePos.y), new vf2d(vScreenSpacePos.x, vScreenSpaceDim.y), new vf2d(vScreenSpaceDim.x, vScreenSpaceDim.y), new vf2d(vScreenSpaceDim.x, vScreenSpacePos.y) };
            vf2d uvtl = source_pos * decal.vUVScale;
            vf2d uvbr = uvtl + (source_size * decal.vUVScale);
            di.uv = new List<vf2d>() { new vf2d(uvtl.x, uvtl.y), new vf2d(uvtl.x, uvbr.y), new vf2d(uvbr.x, uvbr.y), new vf2d(uvbr.x, uvtl.y) };
            di.w = new List<float>() { 1, 1, 1, 1 };
            di.mode = nDecalMode;
            vLayers[nTargetLayer].vecDecalInstance.Add(di);
        }

        void DrawPartialDecal(vf2d pos, vf2d size, Decal decal, vf2d source_pos, vf2d source_size, Pixel tint)
        {
            vf2d vScreenSpacePos = new vf2d(
                (float)((Math.Floor(pos.x) * vInvScreenSize.x) * 2.0f - 1.0f),
                (float)(((Math.Floor(pos.y) * vInvScreenSize.y) * 2.0f - 1.0f) * -1.0f)
            );
            vf2d vScreenSpaceDim = new vf2d(
                vScreenSpacePos.x + (2.0f * size.x * vInvScreenSize.x),
                (vScreenSpacePos.y - (2.0f * size.y * vInvScreenSize.y))
            );

            DecalInstance di;
            di.points = 4;
            di.decal = decal;
            di.tint = new List<Pixel>() { tint, tint, tint, tint };
            di.pos = new List<vf2d>() { new vf2d(vScreenSpacePos.x, vScreenSpacePos.y), new vf2d(vScreenSpacePos.x, vScreenSpaceDim.y), new vf2d(vScreenSpaceDim.x, vScreenSpaceDim.y), new vf2d(vScreenSpaceDim.x, vScreenSpacePos.y) };
            vf2d uvtl = (source_pos) * decal.vUVScale;
            vf2d uvbr = uvtl + ((source_size) * decal.vUVScale);
            di.uv = new List<vf2d>() { new vf2d(uvtl), new vf2d(uvtl), new vf2d(uvbr), new vf2d(uvbr) };
            di.w = new List<float>() { 1, 1, 1, 1 };
            di.mode = nDecalMode;
            vLayers[nTargetLayer].vecDecalInstance.Add(di);
        }

        private void DrawPartialDecal(vf2d vf2d, Decal fontDecal, List<vf2d> vf2ds, vf2d scale, Pixel col)
        {
            throw new NotImplementedException();
        }


        // Draws fully user controlled 4 vertices, pos(pixels), uv(pixels), colours
        void DrawExplicitDecal(Decal decal, List<vf2d> pos, List<vf2d> uv, List<Pixel> col, int elements = 4)
        {
            DecalInstance di;
            di.decal = decal;
            di.pos = new List<vf2d>(elements);
            di.uv = new List<vf2d>(elements);
            di.w = new List<float>(elements);
            di.tint = new List<Pixel>(elements);
            di.points = (uint)elements;
            for (int i = 0; i < elements; i++)
            {
                di.pos[i] = new vf2d((float)((pos[i].x * vInvScreenSize.x) * 2.0f - 1.0f), (float)(((pos[i].y * vInvScreenSize.y) * 2.0f - 1.0f) * -1.0f));
                di.uv[i] = uv[i];
                di.tint[i] = col[i];
                di.w[i] = 1.0f;
            }
            di.mode = nDecalMode;
            vLayers[nTargetLayer].vecDecalInstance.Add(di);
        }
        // Draws a decal with 4 arbitrary points, warping the texture to look "correct"
        void DrawWarpedDecal(Decal decal, vf2d[] pos, Pixel tint) => DrawWarpedDecal(decal, pos, tint);
        //void PixelGameEngine::DrawWarpedDecal(olc::Decal* decal, const olc::vf2d(&pos)[4], const olc::Pixel& tint)
        //void PixelGameEngine::DrawPartialWarpedDecal(olc::Decal* decal, const std::array<olc::vf2d, 4>& pos, const olc::vf2d& source_pos, const olc::vf2d& source_size, const olc::Pixel& tint)
        void DrawWarpedDecal(Decal decal, List<vf2d> pos, Pixel tint)
        {
            // Thanks Nathan Reed, a brilliant article explaining whats going on here
            // http://www.reedbeta.com/blog/quadrilateral-interpolation-part-1/
            DecalInstance di;
            di.points = 4;
            di.decal = decal;
            di.tint = new List<Pixel>() { tint, tint, tint, tint };
            di.w = new List<float>() { 1, 1, 1, 1 };
            di.pos = new List<vf2d>(4);
            di.uv = new List<vf2d>() { new vf2d(0.0f, 0.0f), new vf2d(0.0f, 1.0f), new vf2d(1.0f, 1.0f), new vf2d(1.0f, 0.0f) };
            vf2d center = new vf2d();
            float rd = ((pos[2].x - pos[0].x) * (pos[3].y - pos[1].y) - (pos[3].x - pos[1].x) * (pos[2].y - pos[0].y));
            if (rd != 0)
            {
                rd = 1.0f / rd;
                float rn = ((pos[3].x - pos[1].x) * (pos[0].y - pos[1].y) - (pos[3].y - pos[1].y) * (pos[0].x - pos[1].x)) * rd;
                float sn = ((pos[2].x - pos[0].x) * (pos[0].y - pos[1].y) - (pos[2].y - pos[0].y) * (pos[0].x - pos[1].x)) * rd;
                if (!(rn < 0.0f || rn > 1.0f || sn < 0.0f || sn > 1.0f)) center = pos[0] + rn * (pos[2] - pos[0]);
                float[] d = new float[4]; for (int i = 0; i < 4; i++) d[i] = (float)((pos[i] - center).mag());
                for (int i = 0; i < 4; i++)
                {
                    float q = d[i] == 0.0f ? 1.0f : (d[i] + d[(i + 2) & 3]) / d[(i + 2) & 3];
                    di.uv[i] *= q; di.w[i] *= q;
                    di.pos[i] = new vf2d((pos[i].x * vInvScreenSize.x) * 2.0f - 1.0f, ((pos[i].y * vInvScreenSize.y) * 2.0f - 1.0f) * -1.0f);
                }
                di.mode = nDecalMode;
                vLayers[nTargetLayer].vecDecalInstance.Add(di);
            }
        }
        // As above, but you can specify a region of a decal source sprite
        //void DrawPartialWarpedDecal(Decal* decal, vf2d(&pos)[4],vf2d& source_pos,vf2d& source_size,Pixel& tint = Pixel.WHITE);
        //void DrawPartialWarpedDecal(Decal* decal, std::array<vf2d, 4>& pos, vf2d& source_pos, vf2d& source_size, Pixel& tint = Pixel.WHITE);
        
        void DrawPartialWarpedDecal(Decal decal, List<vf2d> pos, vf2d source_pos, vf2d source_size, Pixel tint)
        {
            DecalInstance di;
            di.points = 4;
            di.decal = decal;
            di.tint = new List<Pixel>() { tint, tint, tint, tint };
            di.w = new List<float>() { 1, 1, 1, 1 };
            di.pos = new List<vf2d>(4);
            di.uv = new List<vf2d>() { new vf2d(0.0f, 0.0f), new vf2d(0.0f, 1.0f), new vf2d(1.0f, 1.0f), new vf2d(1.0f, 0.0f) };
            vf2d center = new vf2d();
            float rd = ((pos[2].x - pos[0].x) * (pos[3].y - pos[1].y) - (pos[3].x - pos[1].x) * (pos[2].y - pos[0].y));
            if (rd != 0)
            {
                vf2d uvtl = source_pos * decal.vUVScale;
                vf2d uvbr = uvtl + (source_size * decal.vUVScale);
                di.uv = new List<vf2d>() { new vf2d(uvtl), new vf2d(uvtl), new vf2d(uvbr), new vf2d(uvbr) };

                rd = 1.0f / rd;
                float rn = ((pos[3].x - pos[1].x) * (pos[0].y - pos[1].y) - (pos[3].y - pos[1].y) * (pos[0].x - pos[1].x)) * rd;
                float sn = ((pos[2].x - pos[0].x) * (pos[0].y - pos[1].y) - (pos[2].y - pos[0].y) * (pos[0].x - pos[1].x)) * rd;
                if (!(rn < 0.0f || rn > 1.0f || sn < 0.0f || sn > 1.0f)) center = pos[0] + rn * (pos[2] - pos[0]);
                float[] d = new float[4]; for (int i = 0; i < 4; i++) d[i] = (float)(pos[i] - center).mag();
                for (int i = 0; i < 4; i++)
                {
                    float q = d[i] == 0.0f ? 1.0f : (d[i] + d[(i + 2) & 3]) / d[(i + 2) & 3];
                    di.uv[i] *= q; di.w[i] *= q;
                    di.pos[i] = new vf2d((pos[i].x * vInvScreenSize.x) * 2.0f - 1.0f, ((pos[i].y * vInvScreenSize.y) * 2.0f - 1.0f) * -1.0f);
                }
                di.mode = nDecalMode;
                vLayers[nTargetLayer].vecDecalInstance.Add(di);
            }
        }
        // Draws a decal rotated to specified angle, wit point of rotation offset
        void DrawRotatedDecal(vf2d pos, Decal decal, float fAngle, vf2d center, vf2d scale, Pixel tint)
        {
            DecalInstance di;
            di.decal = decal;
            di.uv = new List<vf2d>() { new vf2d(0.0f, 0.0f), new vf2d(0.0f, 1.0f), new vf2d(1.0f, 1.0f), new vf2d(1.0f, 0.0f) };
            di.w = new List<float>() { 1, 1, 1, 1 };
            di.tint = new List<Pixel>() { tint, tint, tint, tint };
            di.points = 4;
            di.pos = new List<vf2d>(4);
            di.pos[0] = (new vf2d(0.0f, 0.0f) - center) * scale;
            di.pos[1] = (new vf2d(0.0f, (float)(decal.sprite.height)) - center) * scale;
            di.pos[2] = (new vf2d((float)(decal.sprite.width), (float)(decal.sprite.height)) - center) * scale;
            di.pos[3] = (new vf2d((float)(decal.sprite.width), 0.0f) - center) * scale;
            float c = (float)Math.Cos(fAngle), s = (float)Math.Sin(fAngle);
            for (int i = 0; i < 4; i++)
            {
                di.pos[i] = pos + new vf2d(di.pos[i].x * c - di.pos[i].y * s, di.pos[i].x * s + di.pos[i].y * c);
                di.pos[i] = di.pos[i] * vInvScreenSize * 2.0f - new vf2d(1.0f, 1.0f);
                di.pos[i].y *= -1.0f;
                di.w[i] = 1;
            }
            di.mode = nDecalMode;
            vLayers[nTargetLayer].vecDecalInstance.Add(di);
        }

        void DrawPartialRotatedDecal(vf2d pos, Decal decal, float fAngle, vf2d center, vf2d source_pos, vf2d source_size, vf2d scale, Pixel tint)
        {
            DecalInstance di;
            di.decal = decal;
            di.points = 4;
            di.tint = new List<Pixel>() { tint, tint, tint, tint };
            di.w = new List<float>() { 1, 1, 1, 1 };
            di.pos = new List<vf2d>(4);
            di.pos[0] = (new vf2d(0.0f, 0.0f) - center) * scale;
            di.pos[1] = (new vf2d(0.0f, source_size.y) - center) * scale;
            di.pos[2] = (new vf2d(source_size.x, source_size.y) - center) * scale;
            di.pos[3] = (new vf2d(source_size.x, 0.0f) - center) * scale;
            float c = (float)Math.Cos(fAngle), s = (float)Math.Sin(fAngle);
            for (int i = 0; i < 4; i++)
            {
                di.pos[i] = pos + new vf2d(di.pos[i].x * c - di.pos[i].y * s, di.pos[i].x * s + di.pos[i].y * c);
                di.pos[i] = di.pos[i] * vInvScreenSize * 2.0f - new vf2d(1.0f, 1.0f);
                di.pos[i].y *= -1.0f;
            }

            vf2d uvtl = source_pos * decal.vUVScale;
            vf2d uvbr = uvtl + (source_size * decal.vUVScale);
            di.uv = new List<vf2d>() { new vf2d(uvtl), new vf2d(uvtl), new vf2d(uvbr), new vf2d(uvbr) };
            di.mode = nDecalMode;
            vLayers[nTargetLayer].vecDecalInstance.Add(di);
        }
        // Draws a multiline string as a decal, with tiniting and scaling
        void DrawStringDecal(vf2d pos, string sText, Pixel col, vf2d scale)
        {
            vf2d spos = new vf2d(0.0f, 0.0f);
            foreach (var c in sText)
            {
                if (c == '\n')
                {
                    spos.x = 0; spos.y += 8.0f * scale.y;
                }
                else
                {
                    int ox = (c - 32) % 16;
                    int oy = (c - 32) / 16;
                    DrawPartialDecal(pos + spos, fontDecal, new List<vf2d>() { new vf2d((float)(ox) * 8.0f, (float)(oy) * 8.0f), new vf2d(8.0f, 8.0f) }, scale, col);
                    spos.x += 8.0f * scale.x;
                }
            }
        }


        void DrawStringPropDecal(vf2d pos, string sText, Pixel col, vf2d scale)
        {
            vf2d spos = new vf2d(0.0f, 0.0f);
            foreach (var c in sText)
            {
                if (c == '\n')
                {
                    spos.x = 0; spos.y += 8.0f * scale.y;
                }
                else
                {
                    int ox = (c - 32) % 16;
                    int oy = (c - 32) / 16;
                    DrawPartialDecal(pos + spos, fontDecal, new List<vf2d>() { new vf2d((float)(ox) * 8.0f + (float)(vFontSpacing[c - 32].x), (float)(oy) * 8.0f), new vf2d((float)(vFontSpacing[c - 32].y), 8.0f) }, scale, col);
                    spos.x += (float)(vFontSpacing[c - 32].y) * scale.x;
                }
            }
        }
        // Draws a single shaded filled rectangle as a decal
        void FillRectDecal(vf2d pos, vf2d size, Pixel col)
        {
            List<vf2d> points = new List<vf2d>() { new vf2d(pos), new vf2d(pos.x, pos.y + size.y), new vf2d(pos + size), new vf2d(pos.x + size.x, pos.y) };
            List<vf2d> uvs = new List<vf2d>() { new vf2d(0, 0), new vf2d(0, 0), new vf2d(0, 0), new vf2d(0, 0) };
            List<Pixel> cols = new List<Pixel>() { col, col, col, col };
            DrawExplicitDecal(null, points, uvs, cols, 4);
        }
        // Draws a corner shaded rectangle as a decal
        void GradientFillRectDecal(vf2d pos, vf2d size, Pixel colTL, Pixel colBL, Pixel colBR, Pixel colTR)
        {
            List<vf2d> points = new List<vf2d>() { new vf2d(pos), new vf2d(pos.x, pos.y + size.y), new vf2d(pos + size), new vf2d(pos.x + size.x, pos.y) };
            List<vf2d> uvs = new List<vf2d>() { new vf2d(0, 0), new vf2d(0, 0), new vf2d(0, 0), new vf2d(0, 0) };
            List<Pixel> cols = new List<Pixel>() { colTL, colBL, colBR, colTR };
            DrawExplicitDecal(null, points, uvs, cols, 4);
        }
        // Draws an arbitrary convex textured polygon using GPU
        void DrawPolygonDecal(Decal decal, List<vf2d> pos, List<vf2d> uv, Pixel tint)
        {
            DecalInstance di;
            di.decal = decal;
            di.points = (uint)pos.Count;
            di.pos = new List<vf2d>((int)di.points);
            di.uv = new List<vf2d>((int)di.points);
            di.w = new List<float>((int)di.points);
            di.tint = new List<Pixel>((int)di.points);
            for (int i = 0; i < di.points; i++)
            {
                di.pos[i] = new vf2d((float)((pos[i].x * vInvScreenSize.x) * 2.0f - 1.0f), (float)((pos[i].y * vInvScreenSize.y) * 2.0f - 1.0f * -1.0f)); // possible bug.. should the last element be on the whole thing?
                di.uv[i] = uv[i];
                di.tint[i] = tint;
                di.w[i] = 1.0f;
            }
            di.mode = nDecalMode;
            vLayers[nTargetLayer].vecDecalInstance.Add(di);
        }

        // Clears entire draw target to Pixel
        public void Clear(Pixel p)
        {
            int pixels = (int)(GetDrawTargetWidth() * GetDrawTargetHeight());
            List<Pixel> m = GetDrawTarget().GetData();
            for (int i = 0; i < pixels; i++) m[i] = p;
        }
        // Clears the rendering back buffer
        public void ClearBuffer(Pixel p, bool bDepth = true) => renderer.ClearBuffer(p, bDepth);
        // Returns the font image
        public Sprite GetFontSprite() => fontSprite;

        // Branding
        public string sAppName;


        Sprite pDrawTarget = null;
        PixelMode nPixelMode = PixelMode.NORMAL;
        float fBlendFactor = 1.0f;
        vi2d vScreenSize = new vi2d(256, 240);
        vf2d vInvScreenSize = new vf2d(1.0f / 256.0f, 1.0f / 240.0f);
        vi2d vPixelSize = new vi2d(4, 4);
        vi2d vScreenPixelSize = new vi2d(4, 4);
        vi2d vMousePos = new vi2d(0, 0);
        Int32 nMouseWheelDelta = 0;
        vi2d vMousePosCache = new vi2d(0, 0);
        vi2d vMouseWindowPos = new vi2d(0, 0);
        Int32 nMouseWheelDeltaCache = 0;
        vi2d vWindowSize = new vi2d(0, 0);
        vi2d vViewPos = new vi2d(0, 0);
        vi2d vViewSize = new vi2d(0, 0);
        bool bFullScreen = false;
        vf2d vPixel = new vf2d(1.0f, 1.0f);
        bool bHasInputFocus = false;
        bool bHasMouseFocus = false;
        bool bEnableVSYNC = false;
        float fFrameTimer = 1.0f;
        float fLastElapsed = 0.0f;
        int nFrameCount = 0;
        Sprite fontSprite = null;
        Decal fontDecal = null;
        Sprite pDefaultDrawTarget = null;
        List<LayerDesc> vLayers = new List<LayerDesc>();
        byte nTargetLayer = 0;
        int nLastFPS = 0;
        bool bPixelCohesion = false;
        DecalMode nDecalMode = DecalMode.NORMAL;
        Func<int, int, Pixel, Pixel, Pixel> funcPixelMode;
        //std::function<Pixel(const int x,int y, Pixel&, Pixel&)> funcPixelMode;
        //std::chrono::time_point<std::chrono::system_clock> m_tp1, m_tp2;
        List<vi2d> vFontSpacing;

        // State of keyboard		
        bool[] pKeyNewState = new bool[256];
        bool[] pKeyOldState = new bool[256];
        HWButton[] pKeyboardState = new HWButton[256];

        // State of mouse
        bool[] pMouseNewState = new bool[Const.nMouseButtons] { false, false, false, false, false };
        bool[] pMouseOldState = new bool[Const.nMouseButtons] { false, false, false, false, false };
        HWButton[] pMouseState = new HWButton[Const.nMouseButtons];

        // The main engine thread
        void EngineThread()
        {
            // Allow platform to do stuff here if needed, since its now in the
            // context of this thread
            if (platform.ThreadStartUp() == rcode.FAIL) return;

            // Do engine context specific initialisation
            olc_PrepareEngine();

            // Create user resources as part of this thread
            foreach (var ext in vExtensions) ext.OnBeforeUserCreate();
            if (!OnUserCreate()) bAtomActive = false;
            foreach (var ext in vExtensions) ext.OnAfterUserCreate();


            while (bAtomActive)
            {
                // Run as fast as possible
                while (bAtomActive) { olc_CoreUpdate(); }

                // Allow the user to free resources if they have overrided the destroy function
                if (!OnUserDestroy())
                {
                    // User denied destroy for some reason, so continue running
                    bAtomActive = true;
                }
               
            }

            platform.ThreadCleanUp();
        }


        // If anything sets this flag to false, the engine
        // "should" shut down gracefully
        static bool bAtomActive;


        // "Break In" Functions

        public void olc_UpdateMouse(int x, int y)
        {
            // Mouse coords come in screen space
            // But leave in pixel space
            bHasMouseFocus = true;
            vMouseWindowPos = new vi2d(x, y);
            // Full Screen mode may have a weird viewport we must clamp to
            x -= vViewPos.x;
            y -= vViewPos.y;
            vMousePosCache.x = (int)(((float)x / (float)(vWindowSize.x - (vViewPos.x * 2)) * (float)vScreenSize.x));
            vMousePosCache.y = (int)(((float)y / (float)(vWindowSize.y - (vViewPos.y * 2)) * (float)vScreenSize.y));
            if (vMousePosCache.x >= (int)vScreenSize.x) vMousePosCache.x = vScreenSize.x - 1;
            if (vMousePosCache.y >= (int)vScreenSize.y) vMousePosCache.y = vScreenSize.y - 1;
            if (vMousePosCache.x < 0) vMousePosCache.x = 0;
            if (vMousePosCache.y < 0) vMousePosCache.y = 0;
        }
        public void olc_UpdateMouseWheel(Int32 delta) => nMouseWheelDeltaCache += delta;
        public void olc_UpdateWindowSize(Int32 x, Int32 y)
        {
            vWindowSize = new vi2d(x, y);
            olc_UpdateViewport();
            Console.WriteLine("olc_UpdateWindowSize");
        }

        void olc_UpdateViewport()
        {
            int ww = vScreenSize.x * vPixelSize.x;
            int wh = vScreenSize.y * vPixelSize.y;
            float wasp = (float)ww / (float)wh;

            if (bPixelCohesion)
            {
                vScreenPixelSize = (vWindowSize / vScreenSize);
                vViewSize = (vWindowSize / vScreenSize) * vScreenSize;
            }
            else
            {
                vViewSize.x = (int)vWindowSize.x;
                vViewSize.y = (int)((float)vViewSize.x / wasp);

                if (vViewSize.y > vWindowSize.y)
                {
                    vViewSize.y = vWindowSize.y;
                    vViewSize.x = (int)((float)vViewSize.y * wasp);
                }
            }

            vViewPos = (vWindowSize - vViewSize) / 2;
        }
        void olc_ConstructFontSheet()
        {
            string data = "";
            data += "?Q`0001oOch0o01o@F40o0<AGD4090LAGD<090@A7ch0?00O7Q`0600>00000000";
            data += "O000000nOT0063Qo4d8>?7a14Gno94AA4gno94AaOT0>o3`oO400o7QN00000400";
            data += "Of80001oOg<7O7moBGT7O7lABET024@aBEd714AiOdl717a_=TH013Q>00000000";
            data += "720D000V?V5oB3Q_HdUoE7a9@DdDE4A9@DmoE4A;Hg]oM4Aj8S4D84@`00000000";
            data += "OaPT1000Oa`^13P1@AI[?g`1@A=[OdAoHgljA4Ao?WlBA7l1710007l100000000";
            data += "ObM6000oOfMV?3QoBDD`O7a0BDDH@5A0BDD<@5A0BGeVO5ao@CQR?5Po00000000";
            data += "Oc``000?Ogij70PO2D]??0Ph2DUM@7i`2DTg@7lh2GUj?0TO0C1870T?00000000";
            data += "70<4001o?P<7?1QoHg43O;`h@GT0@:@LB@d0>:@hN@L0@?aoN@<0O7ao0000?000";
            data += "OcH0001SOglLA7mg24TnK7ln24US>0PL24U140PnOgl0>7QgOcH0K71S0000A000";
            data += "00H00000@Dm1S007@DUSg00?OdTnH7YhOfTL<7Yh@Cl0700?@Ah0300700000000";
            data += "<008001QL00ZA41a@6HnI<1i@FHLM81M@@0LG81?O`0nC?Y7?`0ZA7Y300080000";
            data += "O`082000Oh0827mo6>Hn?Wmo?6HnMb11MP08@C11H`08@FP0@@0004@000000000";
            data += "00P00001Oab00003OcKP0006@6=PMgl<@440MglH@000000`@000001P00000000";
            data += "Ob@8@@00Ob@8@Ga13R@8Mga172@8?PAo3R@827QoOb@820@0O`0007`0000007P0";
            data += "O`000P08Od400g`<3V=P0G`673IP0`@3>1`00P@6O`P00g`<O`000GP800000000";
            data += "?P9PL020O`<`N3R0@E4HC7b0@ET<ATB0@@l6C4B0O`H3N7b0?P01L3R000000020";

            fontSprite = new Sprite(128, 48);
            int px = 0, py = 0;
            for (int b = 0; b < 1024; b += 4)
            {
                uint sym1 = (uint)data[b + 0] - 48;
                uint sym2 = (uint)data[b + 1] - 48;
                uint sym3 = (uint)data[b + 2] - 48;
                uint sym4 = (uint)data[b + 3] - 48;
                uint r = sym1 << 18 | sym2 << 12 | sym3 << 6 | sym4;

                for (int i = 0; i < 24; i++)
                {
                    int k = (r & (1 << i)) != 0 ? 255 : 0;
                    fontSprite.SetPixel(px, py, new Pixel(k, k, k, k));
                    if (++py == 48) { px++; py = 0; }
                }
            }

            fontDecal = new Decal(fontSprite);

            byte[] vSpacing = new byte[96]
                {
                    0x03,0x25,0x16,0x08,0x07,0x08,0x08,0x04,0x15,0x15,0x08,0x07,0x15,0x07,0x24,0x08,
                    0x08,0x17,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x24,0x15,0x06,0x07,0x16,0x17,
                    0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x17,0x08,0x08,0x17,0x08,0x08,0x08,
                    0x08,0x08,0x08,0x08,0x17,0x08,0x08,0x08,0x08,0x17,0x08,0x15,0x08,0x15,0x08,0x08,
                    0x24,0x18,0x17,0x17,0x17,0x17,0x17,0x17,0x17,0x33,0x17,0x17,0x33,0x18,0x17,0x17,
                    0x17,0x17,0x17,0x17,0x07,0x17,0x17,0x18,0x18,0x17,0x17,0x07,0x33,0x07,0x08,0x00
                };
            vFontSpacing = new List<vi2d>();
            foreach (var c in vSpacing) vFontSpacing.Add(new vi2d(c >> 4, c & 15));
        }

        DateTime m_tp2;
        DateTime m_tp1;
        int tick =0;
        void olc_CoreUpdate()
        {
            //Console.WriteLine(Const.outputlog());
            //tick++;
            // Handle Timing
            //m_tp2 = std::chrono::system_clock::now();
            m_tp2 = clock.UtcNow;
            
            //std::chrono::duration<float> elapsedTime = m_tp2 - m_tp1;
            float elapsedTime = (float)(( m_tp2 - m_tp1).Milliseconds)/1000;
            m_tp1 = m_tp2;
            //mtp1 = 
            // Our time per frame coefficient
            //float fElapsedTime = elapsedTime.count();
            float fElapsedTime = (float)elapsedTime;

            fLastElapsed = fElapsedTime;

            // Some platforms will need to check for events
            platform.HandleSystemEvent();

            // Compare hardware input states from previous frame

            // auto ScanHardware = [&](HWButton * pKeys, bool * pStateOld, bool * pStateNew, uint32_t nKeyCount)
            void ScanHardware(HWButton[] pKeys, bool[] pStateOld, bool[] pStateNew, int nKeyCount)
            {
                for (int i = 0; i < nKeyCount; i++)
                {
                    pKeys[i].bPressed = false;
                    pKeys[i].bReleased = false;
                    if (pStateNew[i] != pStateOld[i])
                    {
                        if (pStateNew[i])
                        {
                            pKeys[i].bPressed = !pKeys[i].bHeld;
                            pKeys[i].bHeld = true;
                        }
                        else
                        {
                            pKeys[i].bReleased = true;
                            pKeys[i].bHeld = false;
                        }
                    }
                    pStateOld[i] = pStateNew[i];
                }
            };

            ScanHardware(pKeyboardState, pKeyOldState, pKeyNewState, 256);
            ScanHardware(pMouseState, pMouseOldState, pMouseNewState, nMouseButtons);

            // Cache mouse coordinates so they remain consistent during frame
            vMousePos = vMousePosCache;
            nMouseWheelDelta = nMouseWheelDeltaCache;
            nMouseWheelDeltaCache = 0;

            //	renderer->ClearBuffer(olc::BLACK, true);

            // Handle Frame Update
            //for (auto & ext : vExtensions) ext->OnBeforeUserUpdate(fElapsedTime);
            foreach (var ext in vExtensions) ext.OnBeforeUserUpdate(fElapsedTime);

            if (!OnUserUpdate(fElapsedTime)) bAtomActive = false;

            //for (auto & ext : vExtensions) ext->OnAfterUserUpdate(fElapsedTime);
            foreach (var ext in vExtensions) ext.OnAfterUserUpdate(fElapsedTime);


            // Display Frame
            renderer.UpdateViewport(vViewPos, vViewSize);
            renderer.ClearBuffer(BLACK, true);

            // Layer 0 must always exist
            vLayers[0].bUpdate = true;
            vLayers[0].bShow = true;
            SetDecalMode(DecalMode.NORMAL);
            renderer.PrepareDrawing();

            //for (auto layer = vLayers.rbegin(); layer != vLayers.rend(); ++layer)
            //for (int i = vLayers.)
            foreach (var layer in vLayers) // vLayers.rend() not implemented / taken into account (Meaning array needs to be traversed in reverse)
            {
                if (layer.bShow)
                {
                    if (layer.funcHook == null)
                    {
                        renderer.ApplyTexture(layer.nResID);
                        if (layer.bUpdate)
                        {
                            renderer.UpdateTexture(layer.nResID, layer.pDrawTarget);
                            layer.bUpdate = false;
                        }

                        renderer.DrawLayerQuad(layer.vOffset, layer.vScale, layer.tint);

                        // Display Decals in order for this layer
                        //for (auto & decal : layer->vecDecalInstance)
                        foreach (var decal in layer.vecDecalInstance)
                            renderer.DrawDecal(decal);
                        layer.vecDecalInstance.Clear();
                    }
                    else
                    {
                        // Mwa ha ha.... Have Fun!!!
                        layer.funcHook();
                    }
                }
            }

            // Present Graphics to screen
            renderer.DisplayFrame();

            // Update Title Bar
            fFrameTimer += fElapsedTime;
            nFrameCount++;

            if (fFrameTimer >= 1.0f)
            {                
                nLastFPS = nFrameCount;
                fFrameTimer -= 1.0f;
                string sTitle = "OneLoneCoder.com - Pixel Game Engine - " + sAppName + " - FPS: " + nFrameCount.ToString();
                platform.SetWindowTitle(sTitle);
                nFrameCount = 0;
            }
        }
        void olc_PrepareEngine()
        {
            // Start OpenGL, the context is owned by the game thread
            if (platform.CreateGraphics(bFullScreen, bEnableVSYNC, vViewPos, vViewSize) == rcode.FAIL) return;

            // Construct default font sheet
            olc_ConstructFontSheet();

            // Create Primary Layer "0"
            CreateLayer();
            vLayers[0].bUpdate = true;
            vLayers[0].bShow = true;
            SetDrawTarget(null);
        }

        public void olc_UpdateMouseState(int button, bool state) => pMouseNewState[button] = state;
        public void olc_UpdateKeyState(int key, bool state) => pKeyNewState[key] = state;
        public void olc_UpdateMouseFocus(bool state) => bHasMouseFocus = state;
        public void olc_UpdateKeyFocus(bool state) => bHasInputFocus = state;
        public void olc_Terminate() => bAtomActive = false;
        public void olc_Reanimate() => bAtomActive = true;
        public bool olc_IsRunning() => bAtomActive;

        // At the very end of this file, chooses which
        // components to compile
        void olc_ConfigureSystem()
        {


            renderer = new Renderer_OGL33();
            platform = new Platform_Windows();


            // Associate components with PGE instance
            Platform.PGE = this;
            Renderer.PGE = this;
        }


        // NOTE: Items Here are to be deprecated, I have left them in for now
        // in case you are using them, but they will be removed.
        // vf2d	vSubPixelOffset = { 0.0f, 0.0f };

        public void pgex_Register(PGEX pgex)
        {
            /*
            	if (std::find(vExtensions.begin(), vExtensions.end(), pgex) == vExtensions.end())
			        vExtensions.push_back(pgex);			
            */
            if (!vExtensions.Contains(pgex)) vExtensions.Add(pgex);
        }

        //private:
        List<PGEX> vExtensions = new List<PGEX>();
    }
}
