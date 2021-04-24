using static olc.Const;

namespace olc
{
    public abstract class Platform
    {
        public abstract rcode ApplicationStartUp();
        public abstract rcode ApplicationCleanUp();
        public abstract rcode ThreadStartUp();
        public abstract rcode ThreadCleanUp();
        public abstract rcode CreateGraphics(bool bFullScreen, bool bEnableVSYNC, vi2d vViewPos, vi2d vViewSize);
        public abstract rcode CreateWindowPane(vi2d vWindowPos, vi2d vWindowSize, bool bFullScreen);
        public abstract rcode SetWindowTitle(string s);
        public abstract rcode StartSystemEventLoop();
        public abstract rcode HandleSystemEvent();
        public static PixelGameEngine PGE;
    }




}

