using olc;
using System;
using System.Collections.Generic;
using System.Text;
using ImGuiNET;
using Dear_ImGui_Sample;
using System.Numerics;

namespace Test
{
    public class GridClass : PixelGameEngine
    {
        int gridX = 100;
        int gridY = 100;
        int size = 32;

        int mapOffsetX = 0;
        int mapOffsetY = 0;
        int ElementHeight = 150;
        int ElementWidth = 100;

        float scale = 1f;
        int cols = 32/4;
        int rows = 15/3;
        Pixel[,] map;
        int[,] intMap;
        int mapWidth = 60;
        int mapHeight = 60;
        vf2d velocity, offset;
        Sprite spr;
        Decal decal;
        ImGuiController _controller;
        bool moving = false;
        byte mGameLayer;

        public override bool OnUserCreate()
        {

            _controller = new ImGuiController(ScreenWidth(), ScreenHeight(), this);

            velocity = new vf2d(0, 0);
            offset = new vf2d(0, 0);
            map = new Pixel[mapWidth, mapHeight];
            intMap = new int[mapWidth, mapHeight];
            Random rng = new Random();
            for (int y = 0; y < mapHeight; y++)
                for (int x = 0; x < mapWidth; x++)
                {
                    map[x, y] = new Pixel(x / (float)mapWidth, y / (float)mapHeight, 1f - (y / (float)mapHeight), 1f);
                    intMap[x, y] = rng.Next(20);
                }
            spr = new Sprite(0, 0);
            spr.LoadFromFile("assets/spritesheet03.png");
            decal = new Decal(spr);


            mGameLayer = CreateLayer();
            EnableLayer(mGameLayer, true);
            SetLayerCustomRenderFunction(mGameLayer, DrawImGui);

            return true;

        }

        
        public void DrawImGui()
        {           
            //_controller.Prepare();
            _controller.Render();
            //ImGui.NewFrame();
            //ImGuiNET.ImGui.Render();
            //ImGuiNET.ImGui.NewFrame();

            return;
        }

        public override bool OnUserUpdate(float fElapsedTime)
        {
            _controller.Update(null,fElapsedTime);
            //SetDrawTarget(0);
            Clear(new Pixel(0, 0, 0, 0));
            if (GetKey(Key.ESCAPE).bPressed) return false;
            HandleMovement(fElapsedTime);

            gridX = (int)(ElementWidth - (size * scale));
            gridY = (int)(ElementHeight - (size * scale));
            int DisplayWidth = (int)(ScreenWidth() - ElementWidth - gridX + size * scale);
            int DisplayHeight = (int)(ScreenHeight() - ElementHeight - gridY + size * scale);
            cols = (int)(DisplayWidth / (size * scale));
            rows = (int)(DisplayHeight / (size * scale));
            SetDecalMode(DecalMode.ADDITIVE);

            for (int x = 0; x < cols + 1; x++)
            {
                if (x + mapOffsetX > map.GetLength(0) - 1) continue;
                for (int y = 0; y < rows + 1; y++)
                {

                    if (y + mapOffsetY > map.GetLength(1) - 1) continue;
                    else DrawCell(x, y, gridX, gridY, offset, scale, map[x + mapOffsetX, y + mapOffsetY], intMap[x + mapOffsetX, y + mapOffsetY]);
                }

            }

//            DrawDecal(new vf2d(10, 10), decal, new vf2d(1, 1), Pixel.WHITE);
            DrawUI();

      
          
            return true;
        }

        private void DrawSprite(int x, int y, int spritenumb, float scale)
        {
            int sourcePosX = ((spritenumb%10)*size) ;
            int sourcePosY = ((spritenumb/10)*size);
            DrawPartialDecal(new vf2d(x, y), new vf2d(size*scale, size*scale),decal, new vf2d(sourcePosX, sourcePosY), new vf2d(size,size), Pixel.WHITE);
            
           // DrawPartialSprite(x, y, spr, sourcePosX, sourcePosY, 32, 32, scale);
        }

        private void DrawUI()
        {
            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.SetNextWindowSize(new Vector2(ScreenWidth(), 150));
            ImGui.Begin("Test", ImGuiWindowFlags.AlwaysAutoResize);
            ImGui.DragInt("MapX", ref mapOffsetX);
            ImGui.SameLine();
            ImGui.DragInt("MapY", ref mapOffsetY);
            ImGui.DragInt("GridX", ref gridX);
            ImGui.SameLine();
            ImGui.DragInt("GridY", ref gridY);
            ImGui.DragFloat("Scale", ref scale, 0.1f);
            ImGui.End();
            ImGui.ShowMetricsWindow();
            Pixel col = new Pixel(50, 20, 30, 100);

            FillRect(ScreenWidth() - ElementWidth, 0, ElementWidth, ScreenHeight(), col);
            FillRect(0, ScreenHeight() - ElementHeight, ScreenWidth() - ElementWidth, ElementHeight, col);
            FillRect(0, 0, ElementWidth, ScreenHeight() - ElementHeight, col);
            FillRect(ElementWidth, 0, ScreenWidth() - (ElementWidth * 2), ElementHeight, col);
        }

        private void HandleMovement(float fElapsedTime)
        {
            float mousewheel = GetMouseWheel();
            if (mousewheel != 0)
            {
                scale = scale + mousewheel * 0.01f;
            }
            if (!moving)
            {
                if (GetKey(Key.RIGHT).bPressed && mapOffsetX < mapWidth - cols + 1)
                {
                    velocity.x = 1;
                }
                else if (GetKey(Key.LEFT).bPressed && mapOffsetX > 0)
                {
                    velocity.x = -1;
                }
                if (GetKey(Key.UP).bPressed && mapOffsetY > 0)
                {

                    velocity.y = -1;
                }
                else if (GetKey(Key.DOWN).bPressed && mapOffsetY < mapHeight - rows + 1)
                {
                    velocity.y = 1;
                }

                if (velocity.x != 0 || velocity.y != 0) moving = true;
            }
            else
            {
                offset += (velocity * fElapsedTime * 2);
                if (offset.x > 1 || offset.x < -1 || offset.y > 1 || offset.y < -1)
                {
                    moving = false;
                    mapOffsetX += (int)velocity.x;
                    mapOffsetY += (int)velocity.y;
                    offset.x = 0;
                    offset.y = 0;
                    velocity.x = 0;
                    velocity.y = 0;
                }
            }
        }

        public void DrawCell(int x, int y, int gridX, int gridY, vf2d offset, float scale, Pixel celldata, int mapData)
        {


            int nx = (int)(gridX + x * (size * scale) - ((size * scale) * offset.x));
            int ny = (int)(gridY + y * (size * scale) - ((size * scale) * offset.y));
            //FillRect(nx, ny, (int)(size*scale), (int)(size*scale), celldata);
            DrawSprite(nx, ny, mapData, scale);
            //DrawRect(nx, ny, (int)(size * scale), (int)(size * scale), celldata);
            //DrawString(nx+10,ny+10, (celldata.r).ToString(), Pixel.WHITE);
        }
    }
}
