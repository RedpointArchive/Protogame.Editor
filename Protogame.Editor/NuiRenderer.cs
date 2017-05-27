using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Protogame;
using System.Collections.Generic;

namespace ProtogameUIStylingTest
{
    public class NuiRenderer
    {
        private readonly IAssetReference<FontAsset> _uiSmallFont;
        private readonly IAssetReference<UberEffectAsset> _surfaceEffect;

        public NuiRenderer(IAssetManager assetManager)
        {
            _uiSmallFont = assetManager.Get<FontAsset>("font.UISmall");
            _surfaceEffect = assetManager.Get<UberEffectAsset>("effect.BuiltinSurface");
        }

        // gradient colours:
        //  checkbox ticked top - 198, 240, 200
        //  checkout ticked bottom - 138, 198, 141

        public void RenderButton(IRenderContext renderContext, Rectangle rectangle)
        {
            RenderButtonInternal(renderContext,
                new Rectangle(rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1));
        }

        public void RenderPressedButton(IRenderContext renderContext, Rectangle rectangle)
        {
            RenderPressedButtonInternal(renderContext,
                new Rectangle(rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1));
        }
        
        public void RenderToggledButton(IRenderContext renderContext, Rectangle rectangle)
        {
            RenderToggledButtonInternal(renderContext,
                new Rectangle(rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1));
        }

        private void RenderButtonInternal(IRenderContext renderContext, Rectangle rectangle)
        {
            RenderRoundedRectangle(renderContext, rectangle, 3, GetBackgroundColorAt, false, false);
            RenderRoundedRectangle(renderContext, rectangle, 3, GetBorderColorAt, true, false);
        }

        private void RenderPressedButtonInternal(IRenderContext renderContext, Rectangle rectangle)
        {
            RenderRoundedRectangle(renderContext, rectangle, 3, GetPressedBackgroundColorAt, false, false);
            RenderRoundedRectangle(renderContext, new Rectangle(rectangle.X + 2, rectangle.Y + 3, rectangle.Width - 4, rectangle.Height - 4), 2f, GetPressedDropShadowAt2, true, false);
            RenderRoundedRectangle(renderContext, new Rectangle(rectangle.X + 1, rectangle.Y + 2, rectangle.Width - 2, rectangle.Height - 2), 2.5f, GetPressedDropShadowAt, true, false);
            RenderRoundedRectangle(renderContext, new Rectangle(rectangle.X, rectangle.Y + 1, rectangle.Width, rectangle.Height), 3f, GetPressedDropShadowAtTopCorners, true, false);
            RenderRoundedRectangle(renderContext, rectangle, 3, GetPressedBorderColorAt, true, false);
        }

        private void RenderToggledButtonInternal(IRenderContext renderContext, Rectangle rectangle)
        {
            RenderRoundedRectangle(renderContext, rectangle, 3, GetToggledBackgroundColorAt, false, false);
            RenderRoundedRectangle(renderContext, new Rectangle(rectangle.X + 2, rectangle.Y + 3, rectangle.Width - 4, rectangle.Height - 4), 2f, GetPressedDropShadowAt2, true, false);
            RenderRoundedRectangle(renderContext, new Rectangle(rectangle.X + 1, rectangle.Y + 2, rectangle.Width - 2, rectangle.Height - 2), 2.5f, GetPressedDropShadowAt, true, false);
            RenderRoundedRectangle(renderContext, new Rectangle(rectangle.X, rectangle.Y + 1, rectangle.Width, rectangle.Height), 3f, GetPressedDropShadowAtTopCorners, true, false);
            RenderRoundedRectangle(renderContext, rectangle, 3, GetPressedBorderColorAt, true, false);
        }

        public void RenderTab(IRenderContext renderContext, Rectangle rectangle)
        {
            RenderTabInternal(renderContext,
                new Rectangle(rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1));
        }

        private void RenderTabInternal(IRenderContext renderContext, Rectangle rectangle)
        {
            RenderRoundedRectangle(renderContext, rectangle, 3, GetBackgroundColorAt, false, true);
            RenderRoundedRectangle(renderContext, rectangle, 3, GetBorderColorAt, true, true);
        }

        private delegate Color ColorFetchCallback(float localX, float localY, float width, float height);

        private Color GetBorderColorAt(float localX, float localY, float width, float height)
        {
            return new Color(0f, 0f, 0f, 0.5f);
        }
        
        private Color GetPressedBorderColorAt(float localX, float localY, float width, float height)
        {
            return new Color(0f, 0f, 0f, 0.5f);
        }

        private Color GetPressedDropShadowAt(float localX, float localY, float width, float height)
        {
            var col1 = new Color(0f, 0f, 0f, 0.2f);
            var col2 = new Color(0f, 0f, 0f, 0f);
            if (localY < height - 1f)
            {
                col1 = new Color(0f, 0f, 0f, 0.4f);
                col2 = new Color(0f, 0f, 0f, 0.1f);
            }
            return Color.Lerp(col1, col2, localY / height);
        }

        private Color GetPressedDropShadowAtTopCorners(float localX, float localY, float width, float height)
        {
            if (localY <= 3f)
            {
                return new Color(0f, 0f, 0f, 0.6f);
            }

            return new Color(0f, 0f, 0f, 0f);
        }

        private Color GetPressedDropShadowAt2(float localX, float localY, float width, float height)
        {
            return new Color(0f, 0f, 0f, 0.05f);
        }

        private Color GetBackgroundColorAt(float localX, float localY, float width, float height)
        {
            var col1 = Color.White;
            var col2 = new Color(194, 192, 193, 255);
            return Color.Lerp(col1, col2, localY / height);
        }

        private Color GetPressedBackgroundColorAt(float localX, float localY, float width, float height)
        {
            var col1 = new Color(193, 193, 193, 255);
            var col2 = new Color(153, 153, 153, 255);
            return Color.Lerp(col1, col2, localY / height);
        }

        private Color GetToggledBackgroundColorAt(float localX, float localY, float width, float height)
        {
            var col1 = new Color(64, 64, 64, 255);
            var col2 = new Color(114, 114, 114, 255);
            return Color.Lerp(col1, col2, localY / height);
        }

        private void RenderRoundedRectangle(IRenderContext renderContext, Rectangle rectangle, float pixelCorners, ColorFetchCallback getColor, bool isBorder, bool isTab)
        {
            float topLeftPixelCorners = pixelCorners;
            float topRightPixelCorners = pixelCorners;
            float bottomLeftPixelCorners = pixelCorners;
            float bottomRightPixelCorners = pixelCorners;

            if (isTab)
            {
                bottomLeftPixelCorners = 0;
                bottomRightPixelCorners = 0;
            }
            
            if (_surfaceEffect.IsReady)
            {
                var points = new List<VertexPositionColor>();
                points.Add(new VertexPositionColor(new Vector3(rectangle.X + topLeftPixelCorners, rectangle.Y, 0), getColor(topLeftPixelCorners, 0, rectangle.Width, rectangle.Height)));
                points.Add(new VertexPositionColor(new Vector3(rectangle.X + rectangle.Width - topRightPixelCorners, rectangle.Y, 0), getColor(rectangle.Width - topRightPixelCorners, 0, rectangle.Width, rectangle.Height)));
                if (!isBorder)
                {
                    points.Add(new VertexPositionColor(new Vector3(rectangle.X + rectangle.Width / 2f, rectangle.Y + rectangle.Height / 2f, 0), getColor(rectangle.Width / 2f, rectangle.Height / 2f, rectangle.Width, rectangle.Height)));
                }
                for (var a = 0f; a < MathHelper.PiOver2; a += MathHelper.PiOver2 / 10f)
                {
                    var i = a + MathHelper.PiOver2 * 1;
                    points.Add(new VertexPositionColor(
                        new Vector3(rectangle.X + rectangle.Width - topRightPixelCorners - (float)Math.Cos(i) * topRightPixelCorners, rectangle.Y + topRightPixelCorners - (float)Math.Sin(i) * topRightPixelCorners, 0),
                        getColor(rectangle.Width - topRightPixelCorners - (float)Math.Cos(i) * topRightPixelCorners, topRightPixelCorners - (float)Math.Sin(i) * topRightPixelCorners, rectangle.Width, rectangle.Height)));
                    if (!isBorder)
                    {
                        points.Add(new VertexPositionColor(new Vector3(rectangle.X + rectangle.Width / 2f, rectangle.Y + rectangle.Height / 2f, 0), getColor(rectangle.Width / 2f, rectangle.Height / 2f, rectangle.Width, rectangle.Height)));
                    }
                }
                points.Add(new VertexPositionColor(new Vector3(rectangle.X + rectangle.Width, rectangle.Y + topRightPixelCorners, 0), getColor(rectangle.Width, topRightPixelCorners, rectangle.Width, rectangle.Height)));
                if (!isBorder)
                {
                    points.Add(new VertexPositionColor(new Vector3(rectangle.X + rectangle.Width / 2f, rectangle.Y + rectangle.Height / 2f, 0), getColor(rectangle.Width / 2f, rectangle.Height / 2f, rectangle.Width, rectangle.Height)));
                }
                points.Add(new VertexPositionColor(new Vector3(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height - bottomRightPixelCorners, 0), getColor(rectangle.Width, rectangle.Height - bottomRightPixelCorners, rectangle.Width, rectangle.Height)));
                if (!isBorder)
                {
                    points.Add(new VertexPositionColor(new Vector3(rectangle.X + rectangle.Width / 2f, rectangle.Y + rectangle.Height / 2f, 0), getColor(rectangle.Width / 2f, rectangle.Height / 2f, rectangle.Width, rectangle.Height)));
                }
                for (var a = 0f; a < MathHelper.PiOver2; a += MathHelper.PiOver2 / 10f)
                {
                    var i = a + MathHelper.PiOver2 * 2;
                    points.Add(new VertexPositionColor(
                        new Vector3(rectangle.X + rectangle.Width - bottomRightPixelCorners - (float)Math.Cos(i) * bottomRightPixelCorners, rectangle.Y + rectangle.Height - bottomRightPixelCorners - (float)Math.Sin(i) * bottomRightPixelCorners, 0),
                        getColor(rectangle.Width - bottomRightPixelCorners - (float)Math.Cos(i) * bottomRightPixelCorners, rectangle.Height - bottomRightPixelCorners - (float)Math.Sin(i) * bottomRightPixelCorners, rectangle.Width, rectangle.Height)));
                    if (!isBorder)
                    {
                        points.Add(new VertexPositionColor(new Vector3(rectangle.X + rectangle.Width / 2f, rectangle.Y + rectangle.Height / 2f, 0), getColor(rectangle.Width / 2f, rectangle.Height / 2f, rectangle.Width, rectangle.Height)));
                    }
                }
                points.Add(new VertexPositionColor(new Vector3(rectangle.X + rectangle.Width - bottomRightPixelCorners, rectangle.Y + rectangle.Height, 0), getColor(rectangle.Width - bottomRightPixelCorners, rectangle.Height, rectangle.Width, rectangle.Height)));
                if (!isBorder)
                {
                    points.Add(new VertexPositionColor(new Vector3(rectangle.X + rectangle.Width / 2f, rectangle.Y + rectangle.Height / 2f, 0), getColor(rectangle.Width / 2f, rectangle.Height / 2f, rectangle.Width, rectangle.Height)));
                }
                points.Add(new VertexPositionColor(new Vector3(rectangle.X + bottomLeftPixelCorners, rectangle.Y + rectangle.Height, 0), getColor(bottomLeftPixelCorners, rectangle.Height, rectangle.Width, rectangle.Height)));
                if (!isBorder)
                {
                    points.Add(new VertexPositionColor(new Vector3(rectangle.X + rectangle.Width / 2f, rectangle.Y + rectangle.Height / 2f, 0), getColor(rectangle.Width / 2f, rectangle.Height / 2f, rectangle.Width, rectangle.Height)));
                }
                for (var a = 0f; a < MathHelper.PiOver2; a += MathHelper.PiOver2 / 10f)
                {
                    var i = a + MathHelper.PiOver2 * 3;
                    points.Add(new VertexPositionColor(
                        new Vector3(rectangle.X + bottomLeftPixelCorners - (float)Math.Cos(i) * bottomLeftPixelCorners, rectangle.Y + rectangle.Height - bottomLeftPixelCorners - (float)Math.Sin(i) * bottomLeftPixelCorners, 0),
                        getColor(bottomLeftPixelCorners - (float)Math.Cos(i) * bottomLeftPixelCorners, rectangle.Height - bottomLeftPixelCorners - (float)Math.Sin(i) * bottomLeftPixelCorners, rectangle.Width, rectangle.Height)));
                    if (!isBorder)
                    {
                        points.Add(new VertexPositionColor(new Vector3(rectangle.X + rectangle.Width / 2f, rectangle.Y + rectangle.Height / 2f, 0), getColor(rectangle.Width / 2f, rectangle.Height / 2f, rectangle.Width, rectangle.Height)));
                    }
                }
                points.Add(new VertexPositionColor(new Vector3(rectangle.X, rectangle.Y + rectangle.Height - bottomLeftPixelCorners, 0), getColor(0, rectangle.Height - bottomLeftPixelCorners, rectangle.Width, rectangle.Height)));
                if (!isBorder)
                {
                    points.Add(new VertexPositionColor(new Vector3(rectangle.X + rectangle.Width / 2f, rectangle.Y + rectangle.Height / 2f, 0), getColor(rectangle.Width / 2f, rectangle.Height / 2f, rectangle.Width, rectangle.Height)));
                }
                points.Add(new VertexPositionColor(new Vector3(rectangle.X, rectangle.Y + topLeftPixelCorners, 0), getColor(0, topLeftPixelCorners, rectangle.Width, rectangle.Height)));
                if (!isBorder)
                {
                    points.Add(new VertexPositionColor(new Vector3(rectangle.X + rectangle.Width / 2f, rectangle.Y + rectangle.Height / 2f, 0), getColor(rectangle.Width / 2f, rectangle.Height / 2f, rectangle.Width, rectangle.Height)));
                }
                for (var a = 0f; a < MathHelper.PiOver2; a += MathHelper.PiOver2 / 10f)
                {
                    var i = a + MathHelper.PiOver2 * 0;
                    points.Add(new VertexPositionColor(
                        new Vector3(rectangle.X + topLeftPixelCorners - (float)Math.Cos(i) * topLeftPixelCorners, rectangle.Y + topLeftPixelCorners - (float)Math.Sin(i) * topLeftPixelCorners, 0),
                        getColor(topLeftPixelCorners - (float)Math.Cos(i) * topLeftPixelCorners, topLeftPixelCorners - (float)Math.Sin(i) * topLeftPixelCorners, rectangle.Width, rectangle.Height)));
                    if (!isBorder)
                    {
                        points.Add(new VertexPositionColor(new Vector3(rectangle.X + rectangle.Width / 2f, rectangle.Y + rectangle.Height / 2f, 0), getColor(rectangle.Width / 2f, rectangle.Height / 2f, rectangle.Width, rectangle.Height)));
                    }
                }
                points.Add(new VertexPositionColor(new Vector3(rectangle.X + topLeftPixelCorners, rectangle.Y, 0), getColor(topLeftPixelCorners, 0, rectangle.Width, rectangle.Height)));
                if (!isBorder)
                {
                    points.Add(new VertexPositionColor(new Vector3(rectangle.X + rectangle.Width / 2f, rectangle.Y + rectangle.Height / 2f, 0), getColor(rectangle.Width / 2f, rectangle.Height / 2f, rectangle.Width, rectangle.Height)));
                }
                var pointsArray = points.ToArray();

                renderContext.SpriteBatch.End();

                var effect = _surfaceEffect.Asset.Effects["ColorNoNormals"];
                var effectParameterSet = effect.CreateParameterSet();
                effect.LoadParameterSet(renderContext, effectParameterSet);
                foreach (var pass in effect.NativeEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    if (isBorder)
                    {
                        renderContext.GraphicsDevice.DrawUserPrimitives(
                            PrimitiveType.LineStrip,
                            pointsArray,
                            0,
                            points.Count - 1);
                    }
                    else
                    {
                        renderContext.GraphicsDevice.DrawUserPrimitives(
                            PrimitiveType.TriangleStrip,
                            pointsArray,
                            0,
                            points.Count - 2);
                    }
                }

                var renderPass = renderContext.GetCurrentRenderPass<ICanvasRenderPass>();
                renderContext.SpriteBatch.Begin(renderPass.TextureSortMode);
            }
        }

        /*
        public Texture2D CreateRoundedRectangleTexture(GraphicsDevice graphics, int width, int height, int borderThickness, int borderRadius, int borderShadow, List<Color> backgroundColors, List<Color> borderColors, float initialShadowIntensity, float finalShadowIntensity)
        {
            if (backgroundColors == null || backgroundColors.Count == 0) throw new ArgumentException("Must define at least one background color (up to four).");
            if (borderColors == null || borderColors.Count == 0) throw new ArgumentException("Must define at least one border color (up to three).");
            if (borderRadius < 1) throw new ArgumentException("Must define a border radius (rounds off edges).");
            if (borderThickness < 1) throw new ArgumentException("Must define border thikness.");
            if (borderThickness + borderRadius > height / 2 || borderThickness + borderRadius > width / 2) throw new ArgumentException("Border will be too thick and/or rounded to fit on the texture.");
            if (borderShadow > borderRadius) throw new ArgumentException("Border shadow must be lesser in magnitude than the border radius (suggeted: shadow <= 0.25 * radius).");

            Texture2D texture = new Texture2D(graphics, width, height, false, SurfaceFormat.Color);
            Color[] color = new Color[width * height];

            for (int x = 0; x < texture.Width; x++)
            {
                for (int y = 0; y < texture.Height; y++)
                {
                    switch (backgroundColors.Count)
                    {
                        case 4:
                            Color leftColor0 = Color.Lerp(backgroundColors[0], backgroundColors[1], ((float)y / (width - 1)));
                            Color rightColor0 = Color.Lerp(backgroundColors[2], backgroundColors[3], ((float)y / (height - 1)));
                            color[x + width * y] = Color.Lerp(leftColor0, rightColor0, ((float)x / (width - 1)));
                            break;
                        case 3:
                            Color leftColor1 = Color.Lerp(backgroundColors[0], backgroundColors[1], ((float)y / (width - 1)));
                            Color rightColor1 = Color.Lerp(backgroundColors[1], backgroundColors[2], ((float)y / (height - 1)));
                            color[x + width * y] = Color.Lerp(leftColor1, rightColor1, ((float)x / (width - 1)));
                            break;
                        case 2:
                            color[x + width * y] = Color.Lerp(backgroundColors[0], backgroundColors[1], ((float)x / (width - 1)));
                            break;
                        default:
                            color[x + width * y] = backgroundColors[0];
                            break;
                    }

                    color[x + width * y] = ColorBorder(x, y, width, height, borderThickness, borderRadius, borderShadow, color[x + width * y], borderColors, initialShadowIntensity, finalShadowIntensity);
                }
            }

            texture.SetData<Color>(color);
            return texture;
        }

        private Color ColorBorder(int x, int y, int width, int height, int borderThickness, int borderRadius, int borderShadow, Color initialColor, List<Color> borderColors, float initialShadowIntensity, float finalShadowIntensity)
        {
            Rectangle internalRectangle = new Rectangle((borderThickness + borderRadius), (borderThickness + borderRadius), width - 2 * (borderThickness + borderRadius), height - 2 * (borderThickness + borderRadius));

            if (internalRectangle.Contains(x, y)) return initialColor;

            Vector2 origin = Vector2.Zero;
            Vector2 point = new Vector2(x, y);

            if (x < borderThickness + borderRadius)
            {
                if (y < borderRadius + borderThickness)
                    origin = new Vector2(borderRadius + borderThickness, borderRadius + borderThickness);
                else if (y > height - (borderRadius + borderThickness))
                    origin = new Vector2(borderRadius + borderThickness, height - (borderRadius + borderThickness));
                else
                    origin = new Vector2(borderRadius + borderThickness, y);
            }
            else if (x > width - (borderRadius + borderThickness))
            {
                if (y < borderRadius + borderThickness)
                    origin = new Vector2(width - (borderRadius + borderThickness), borderRadius + borderThickness);
                else if (y > height - (borderRadius + borderThickness))
                    origin = new Vector2(width - (borderRadius + borderThickness), height - (borderRadius + borderThickness));
                else
                    origin = new Vector2(width - (borderRadius + borderThickness), y);
            }
            else
            {
                if (y < borderRadius + borderThickness)
                    origin = new Vector2(x, borderRadius + borderThickness);
                else if (y > height - (borderRadius + borderThickness))
                    origin = new Vector2(x, height - (borderRadius + borderThickness));
            }

            if (!origin.Equals(Vector2.Zero))
            {
                float distance = Vector2.Distance(point, origin);

                if (distance > borderRadius + borderThickness + 1)
                {
                    return Color.Transparent;
                }
                else if (distance > borderRadius + 1)
                {
                    if (borderColors.Count > 2)
                    {
                        float modNum = distance - borderRadius;

                        if (modNum < borderThickness / 2)
                        {
                            return Color.Lerp(borderColors[2], borderColors[1], (float)((modNum) / (borderThickness / 2.0)));
                        }
                        else
                        {
                            return Color.Lerp(borderColors[1], borderColors[0], (float)((modNum - (borderThickness / 2.0)) / (borderThickness / 2.0)));
                        }
                    }


                    if (borderColors.Count > 0)
                        return borderColors[0];
                }
                else if (distance > borderRadius - borderShadow + 1)
                {
                    float mod = (distance - (borderRadius - borderShadow)) / borderShadow;
                    float shadowDiff = initialShadowIntensity - finalShadowIntensity;
                    return DarkenColor(initialColor, ((shadowDiff * mod) + finalShadowIntensity));
                }
            }

            return initialColor;
        }

        private Color DarkenColor(Color color, float shadowIntensity)
        {
            return Color.Lerp(color, Color.Black, shadowIntensity);
        }
        */
    }
}
