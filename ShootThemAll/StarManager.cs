using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;
using System.Collections.Generic;

namespace ShootThemAll
{
    public struct Star
    {
        public Vector2 Position;
        public Color Color;
        public float Size;
        public float Speed;
        public float Alpha;
    }
    public class StarManager
    {
        List<Star> _stars = new List<Star>();

        public StarManager()
        {

        }
        public void AddStar(Vector2 position, Color color, float size, float speed)
        {
            Star star = new Star
            {
                Position = position,
                Color = color,
                Size = size,
                Speed = speed,
                Alpha = 1f
            };
            _stars.Add(star);
        }

        public void GenerateStar(int nbStars, Rectangle rect)
        {
            for (int i = 0; i < nbStars; i++)
            {
                float x = Misc.Rng.Next(0, rect.Width);
                float y = Misc.Rng.Next(0, rect.Height);

                Vector2 pos = new Vector2(x, y);

                Color color = new Color(Misc.Rng.Next(100, 255), Misc.Rng.Next(100, 255), Misc.Rng.Next(100, 255));

                AddStar(pos, color, Misc.Rng.Next(1, 5), Misc.Rng.Next(20, 40) / 10f);
            }
        }

        public void UpdateStars()
        {
            for (int i = 0; i < _stars.Count; i++)
            {
                Star star = _stars[i];
                star.Position.Y += star.Speed;
                //star.Alpha -= 0.01f;
                //if (star.Alpha <= 0)
                //{
                //    _stars.RemoveAt(i);
                //    i--;
                //}
                //else
                //{
                //    _stars[i] = star;
                //}

                star.Alpha = 1f - (1f / star.Speed) - .25f;

                if (star.Position.Y > Screen.Height)
                {
                    star.Position.Y = 0;
                }

                _stars[i] = star;
            }
        }
        public void DrawStars(SpriteBatch batch, Vector2 offset)
        {
            foreach (var star in _stars)
            {
                batch.FillRectangle(star.Position + offset, new Vector2(star.Size), star.Color * star.Alpha);
            }
        }

    }
}
