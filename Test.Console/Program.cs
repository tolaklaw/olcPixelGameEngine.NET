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
            MouseTest pge = new MouseTest();
            pge.Construct(1280, 768, 1, 1,false, false);
            pge.Start();            
            System.Console.WriteLine("Hello World!");
        }
    }
}
