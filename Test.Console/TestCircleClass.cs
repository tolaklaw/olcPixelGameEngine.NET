using Dear_ImGui_Sample;
using ImGuiNET;
using olc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{
    public class TestCircleClass :PixelGameEngine
    {

        float total = 800;
        float radius = 300;
        float delta = 0;
        float factor = 0;
        float speed = 0.11f;
        ImGuiController _controller;
        byte mGameLayer;
        Pixel p;
        public override bool OnUserCreate()
        {
            _controller = new ImGuiController(ScreenWidth(), ScreenHeight(), this);
            
            radius = Math.Min(ScreenWidth(), ScreenHeight()) / 2;
            p = new Pixel(127, 127, 127);


            mGameLayer = CreateLayer();
            EnableLayer(mGameLayer, true);
            SetLayerCustomRenderFunction(mGameLayer, DrawImGui);

            return true;

        }

        private void DrawImGui()
        {
            _controller.Render();
        }

        public override bool OnUserUpdate(float fElapsedTime)
        {            
            _controller.Update(null, fElapsedTime);
            delta = (MathF.PI * 2) / total;
            Clear(Pixel.BLACK);            
            DCircle(new vi2d(ScreenWidth() / 2, ScreenHeight() / 2), radius);
            DrawString(10, 50, factor.ToString(), Pixel.WHITE);
            factor += speed * fElapsedTime;
            ImGui.Begin("Debug");
            ImGui.DragFloat("Total", ref total, 0.1f);
            ImGui.DragFloat("Radius", ref radius, 1);
            ImGui.DragFloat("Factor", ref factor, 1);
            ImGui.DragFloat("Speed", ref speed, 0.01f);

            ImGui.End();
            return true;
        }


        public void DCircle(vi2d pos, float radius)
        {
            for (int i = 0; i < total; i++)
            {
                var res = i * factor;
                DrawLine(getpos(i) + pos, getpos(res) + pos, new Pixel((byte)i,(byte)(255-i),(byte)i));
            }
        }


        public vi2d getpos(float index)
        {
            return new vi2d((int)(radius * MathF.Cos(((index) * delta)+MathF.PI)), 
                            (int)(radius * MathF.Sin(((index) * delta)+MathF.PI)));
        }
    }
}
