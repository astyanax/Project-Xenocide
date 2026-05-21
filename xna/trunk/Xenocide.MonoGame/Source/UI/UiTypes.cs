namespace ProjectXenocide.UI
{
    public struct UiPoint
    {
        public float X;
        public float Y;
        public UiPoint(float x, float y) { X = x; Y = y; }
    }

    public struct UiSize
    {
        public float Width;
        public float Height;
        public UiSize(float w, float h) { Width = w; Height = h; }
    }

    public struct UiRect
    {
        public float Left;
        public float Top;
        public float Right;
        public float Bottom;

        public float Width { get { return Right - Left; } }
        public float Height { get { return Bottom - Top; } }

        public UiRect(float left, float top, float right, float bottom)
        {
            Left = left; Top = top; Right = right; Bottom = bottom;
        }
    }
}
