using Microsoft.Xna.Framework;
using TSMapEditor.GameMath;
using TSMapEditor.Models;

namespace TSMapEditor.Rendering.ObjectRenderers
{
    public sealed class InfantryRenderer : ObjectRenderer<Infantry>
    {
        public InfantryRenderer(RenderDependencies renderDependencies) : base(renderDependencies)
        {
        }

        protected override Color ReplacementColor => Color.Teal;

        protected override CommonDrawParams GetDrawParams(Infantry gameObject)
        {
            return new CommonDrawParams()
            {
                IniName = gameObject.ObjectType.ININame,
                ShapeImage = TheaterGraphics.InfantryTextures[gameObject.ObjectType.Index]
            };
        }

        protected override float GetDepthAddition(Infantry gameObject)
        {
            if (gameObject.High)
            {
                // Add extra depth to the unit so it is rendered above the bridge.
                // Why are we adding exactly this much?
                // Because it happened to work - this is at least currently no smart mathematical formula.
                int height = Constants.CellSizeY * 7;
                return ((height / (float)Map.HeightInPixelsWithCellHeight) * Constants.DownwardsDepthRenderSpace) + (4 * Constants.DepthRenderStep) + Constants.DepthEpsilon * ObjectDepthAdjustments.Vehicle;
            }

            return Constants.DepthEpsilon * ObjectDepthAdjustments.Infantry;
        }

        protected override double GetExtraLight(Infantry gameObject) => Map.Rules.ExtraInfantryLight;

        public override Point2D GetDrawPoint(Infantry gameObject)
        {
            Point2D drawPoint = base.GetDrawPoint(gameObject);
            Point2D subCellOffset = CellMath.GetSubCellOffset(gameObject.SubCell);
            return drawPoint + subCellOffset;
        }

        protected override void Render(Infantry gameObject, Point2D drawPoint, in CommonDrawParams drawParams)
        {
            if (!gameObject.ObjectType.NoShadow)
                DrawShadow(gameObject);

            DrawShapeImage(gameObject, drawParams.ShapeImage, 
                gameObject.GetFrameIndex(drawParams.ShapeImage.GetFrameCount()), 
                Color.White, true, gameObject.GetRemapColor(),
                false, true, drawPoint);
        }
    }
}
