using olc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{
    public class MazeClass : PixelGameEngine
    {
        public class Node
        {

            static Random rng = new Random();

            public Node(PixelGameEngine pge, int x, int y)
            {
                Position = new vi2d(x, y);
                this.pge = pge;
            }
            private PixelGameEngine pge;
            public vi2d Position;
            public Node North;
            public bool ConnectedNorth = false;
            public Node South;
            public bool ConnectedSouth = false;
            public Node East;
            public bool ConnectedEast = false;
            public Node West;
            public bool ConnectedWest = false;
            public bool Visited = false;
            public void Show()
            {
                if (Visited)
                {
                    Pixel col = Pixel.WHITE;
                    //if (WalkerHistory.Contains(this))
                    //{
                    //    col = Pixel.BLUE;
                    //}

                    pge.Draw(Position * 2, Pixel.WHITE);
                    if (ConnectedSouth && South != null)
                        pge.Draw(Position * 2 + new vi2d(0, 1), col);
                    if (ConnectedEast && East != null)
                        pge.Draw(Position * 2 + new vi2d(1, 0), col);
                }
            }

            public Node RandomNeighbour()
            {
                List<Node> candidates = new List<Node>();
                if (North != null && !North.Visited) candidates.Add(North);
                if (South != null && !South.Visited) candidates.Add(South);
                if (East != null && !East.Visited) candidates.Add(East);
                if (West != null && !West.Visited) candidates.Add(West);

                if (candidates.Count == 0) return null;

                Node choosen = candidates[rng.Next(0, candidates.Count)];
                return choosen;
            }

            public bool HasUnvisitedNeighbours()
            {
                if (North != null && !North.Visited) return true;
                if (South != null && !South.Visited) return true;
                if (East != null && !East.Visited) return true;
                if (West != null && !West.Visited) return true;
                return false;
            }
        }

        Node[,] Map;
        Node Walker;
        static Stack<Node> WalkerHistory = new Stack<Node>();

        public override bool OnUserCreate()
        {
            Map = new Node[ScreenWidth() / 2, ScreenHeight() / 2];

            for (int x = 0; x < ScreenWidth() / 2; x++)
                for (int y = 0; y < ScreenHeight() / 2; y++)
                {
                    Map[x, y] = new Node(this, x, y);
                }

            for (int x = 0; x < ScreenWidth() / 2; x++)
                for (int y = 0; y < ScreenHeight() / 2; y++)
                {
                    if (x - 1 >= 0) Map[x, y].West = Map[x - 1, y];
                    if (x + 1 < ScreenWidth() / 2) Map[x, y].East = Map[x + 1, y];
                    if (y - 1 >= 0) Map[x, y].North = Map[x, y - 1];
                    if (y + 1 < ScreenHeight() / 2) Map[x, y].South = Map[x, y + 1];
                }

            Walker = Map[0, 0];
            return true;
        }

        float time = 0;

        public override bool OnUserUpdate(float fElapsedTime)
        {
            Clear(Pixel.BLACK);
            foreach (Node n in Map)
            {
                n.Show();
            }
            time += fElapsedTime;

            if (time > 0)
            {
                DrawString(1, 1, WalkerHistory.Count.ToString(), Pixel.RED);
                for (int its = 0; its < 50; its++)
                {
                    time = 0;
                    Walker.Visited = true;
                    var chosenNode = Walker.RandomNeighbour();
                    if (chosenNode == null)
                    {
                        if (WalkerHistory.Count > 0)
                            Walker = WalkerHistory.Pop();
                    }
                    else
                    {
                        if (chosenNode == Walker.North)
                        {
                            Walker.ConnectedNorth = true;
                            chosenNode.ConnectedSouth = true;
                        }
                        if (chosenNode == Walker.South)
                        {
                            Walker.ConnectedSouth = true;
                            chosenNode.ConnectedNorth = true;
                        }
                        if (chosenNode == Walker.East)
                        {
                            Walker.ConnectedEast = true;
                            chosenNode.ConnectedWest = true;
                        }

                        if (chosenNode == Walker.West)
                        {
                            Walker.ConnectedWest = true;
                            chosenNode.ConnectedEast = true;

                        }
                        WalkerHistory.Push(Walker);
                        Walker = chosenNode;
                    }
                }
            }
            Draw(Walker.Position * 2, Pixel.GREEN);
            return true;
        }

    }
}
