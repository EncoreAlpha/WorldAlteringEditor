using TSMapEditor.GameMath;

namespace TSMapEditor.Rendering
{
    internal struct AlphaImageRenderStruct
    {
        public Point2D Point;
        public ShapeImage AlphaImage;

        public AlphaImageRenderStruct(Point2D point, ShapeImage alphaImage)
        {
            Point = point;
            AlphaImage = alphaImage;
        }
    }
}
