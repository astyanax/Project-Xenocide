namespace ProjectXenocide.UI
{
    public struct UiPoint
    {
        public float X { get; set; }
        public float Y { get; set; }
        public UiPoint(float x, float y) { X = x; Y = y; }
    }

    public struct UiSize
    {
        public float Width { get; set; }
        public float Height { get; set; }
        public UiSize(float w, float h) { Width = w; Height = h; }
    }

    public struct UiRect
    {
        public float Left { get; set; }
        public float Top { get; set; }
        public float Right { get; set; }
        public float Bottom { get; set; }

        public float Width { get { return Right - Left; } }
        public float Height { get { return Bottom - Top; } }

        public UiRect(float left, float top, float right, float bottom)
        {
            Left = left; Top = top; Right = right; Bottom = bottom;
        }
    }
}
