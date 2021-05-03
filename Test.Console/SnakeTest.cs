using olc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{
    public class SnakeTest : PixelGameEngine
    {

        vi2d SnakeHead;
        vi2d SnakeVel;
        vi2d LastVel;
        List<vi2d> SnakeTail;
        vi2d Food;
        float time;
        Random rng;
        int uiHeight = 10;
        bool gameOver = true;
        int highScore = 0;
        public override bool OnUserCreate()
        {
            rng = new Random();
            InitializeGame();
            return true;

        }

        private vi2d RandomPosition()
        {
            return new vi2d(rng.Next(0, ScreenWidth()), rng.Next(uiHeight + 1, ScreenHeight()));
        }


        private void InitializeGame()
        {
            SnakeHead = new vi2d(ScreenWidth() / 2, ScreenHeight() / 2);
            SnakeTail = new List<vi2d>();
            for (int i = 0; i < 5; i++) SnakeTail.Add(SnakeHead);
            SnakeVel = new vi2d(1, 0);
            Food = new vi2d(ScreenWidth() / 2 + 10, ScreenHeight() / 2);
            time = 0;
        }

        private void DisplayUI()
        {
            DrawRect(0, 0, ScreenWidth() - 1, uiHeight, Pixel.BLUE);
            DrawString(10, 2, (SnakeTail.Count - 5).ToString(), Pixel.WHITE);
            DrawString(50, 2, (highScore).ToString(), Pixel.WHITE);

        }


        public override bool OnUserUpdate(float fElapsedTime)
        {
            Clear(new Pixel(50, 50, 50));
            if (gameOver)
            {
                DrawString(10, 5, "Press", Pixel.WHITE);
                DrawString(10, 15, "SPACE", Pixel.WHITE);
                DrawString(10, 25, "to Play", Pixel.WHITE);
                InitializeGame();
                if (GetKey(Key.SPACE).bPressed) gameOver = false;
            }
            else
            {
                DisplayUI();
                time += fElapsedTime;

                if (GetKey(Key.RIGHT).bPressed && LastVel.x == 0)
                    SnakeVel = new vi2d(1, 0);
                else if (GetKey(Key.LEFT).bPressed && LastVel.x == 0)
                    SnakeVel = new vi2d(-1, 0);
                else if (GetKey(Key.UP).bPressed && LastVel.y == 0)
                    SnakeVel = new vi2d(0, -1);
                else if (GetKey(Key.DOWN).bPressed && LastVel.y == 0)
                    SnakeVel = new vi2d(0, 1);


                Draw(Food, Pixel.RED);

                if (time > 0.05)
                {
                    time = 0;
                    SnakeTail.Insert(0, SnakeHead);
                    SnakeHead += (SnakeVel);
                    LastVel = SnakeVel;
                    if (SnakeHead.x > ScreenWidth() - 1)
                    {
                        SnakeHead.x = 0;
                    }
                    else if (SnakeHead.x < 0)
                    {
                        SnakeHead.x = ScreenWidth() - 1;
                    }
                    else if (SnakeHead.y > ScreenHeight() - 1)
                    {
                        SnakeHead.y = uiHeight + 1;
                    }
                    else if (SnakeHead.y < uiHeight + 1)
                    {
                        SnakeHead.y = ScreenHeight() - 1;
                    }

                    SnakeTail.RemoveAt(SnakeTail.Count - 1);
                    foreach (var tail in SnakeTail)
                    {
                        if (SnakeHead.x == tail.x && SnakeHead.y == tail.y)
                        {
                            gameOver = true;
                            //InitializeGame();
                        }
                    }

                    if (SnakeHead.x == Food.x && SnakeHead.y == Food.y)
                    {
                        for (int i = 0; i < 5; i++) SnakeTail.Add(SnakeHead);
                        Food = RandomPosition();
                    }

                    if (SnakeTail.Count - 5 > highScore)
                    {
                        highScore = SnakeTail.Count - 5;
                    }

                }
                Draw(SnakeHead, Pixel.GREEN);
                foreach (var tail in SnakeTail) Draw(tail, Pixel.WHITE);

                
            }
            return true;
        }

    }
}
