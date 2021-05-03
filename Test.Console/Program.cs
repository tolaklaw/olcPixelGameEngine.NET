#define OLC_GFX_OPENGL33

using System;


namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            
            //OLCTestClass pge = new OLCTestClass();
            //TestCircleClass pge = new TestCircleClass();
            GridClass pge = new GridClass();
            //MouseTest pge = new MouseTest();
            //VectorTest pge = new VectorTest();
            //SnakeTest pge = new SnakeTest();
            //CuboidClass pge = new CuboidClass();
           
            pge.Construct(1280, 768, 1, 1,false, false);

            //pge.Construct(80, 43, 8, 8, false, false);
            pge.Start();            
        }
    }
}
