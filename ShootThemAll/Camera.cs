using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ShootThemAll
{
    public class Camera
    {
        public Vector2 Position => _position;
        private Vector2 _position = Vector2.Zero;
        public Vector2 Origin => _origin;
        private Vector2 _origin = Vector2.Zero;
        public float Zoom => _zoom;
        private float _zoom = 1.0f;
        public float Rotation => _rotation;
        private float _rotation = 0.0f;

        private Viewport _viewPort;

        public Rectangle? Limits
        {
            get { return _limits; }
            set
            {
                if (value != null)
                {
                    // Assign limit but make sure it's always bigger than the viewport
                    _limits = new Rectangle
                    {
                        X = value.Value.X,
                        Y = value.Value.Y,
                        Width = System.Math.Max(_viewPort.Width, value.Value.Width),
                        Height = System.Math.Max(_viewPort.Height, value.Value.Height)
                    };

                    // Validate camera position with new limit
                    _position = Position;
                }
                else
                {
                    _limits = null;
                }
            }
        }
        private Rectangle? _limits;

        public Camera(Viewport viewport)
        {
            _viewPort = viewport;
            _origin = new Vector2(viewport.Width / 2.0f, viewport.Height / 2.0f);
            _zoom = 1.0f;
        }
        public Vector2 WorldToScreen(Vector2 worldPosition, Vector2 parallax)
        {
            return Vector2.Transform(worldPosition, GetViewMatrix(parallax));
        }

        public Vector2 ScreenToWorld(Vector2 screenPosition, Vector2 parallax)
        {
            return Vector2.Transform(screenPosition, Matrix.Invert(GetViewMatrix(parallax)));
        }

        public void SetPosition(float x, float y)
        {
            _position.X = x;
            _position.Y = y;

            // If there's a limit set and the camera is not transformed clamp position to limits
            if (Limits != null && Zoom == 1.0f && Rotation == 0.0f)
            {
                _position.X = MathHelper.Clamp(_position.X, Limits.Value.X, Limits.Value.X + Limits.Value.Width - _viewPort.Width);
                _position.Y = MathHelper.Clamp(_position.Y, Limits.Value.Y, Limits.Value.Y + Limits.Value.Height - _viewPort.Height);
            }

        }
        public void SetPosition(Vector2 position)
        {
            SetPosition(position.X, position.Y);
        }
        public void SetOrigin(float x, float y)
        {
            _origin.X = x;
            _origin.Y = y;
        }
        public void SetOrigin(Vector2 origin)
        {
            _origin = origin;
        }
        public void SetZoom(float zoom)
        {
            _zoom = zoom;
        }
        public void SetRotation(float rotation)
        {
            _rotation = rotation;
        }
        public void Move(float x, float y)
        {
            _position.X += x;
            _position.Y += y;
        }
        public void Move(Vector2 position)
        {
            _position += position;
        }
        public void Rotate(float rotation)
        {
            _rotation += rotation;
        }
        public void Rotate(float rotation, Vector2 origin)
        {
            _rotation += rotation;
            _origin = origin;
        }


        public Matrix GetViewMatrix(Vector2 parallax)
        {
            // To add parallax, simply multiply it by the position
            return Matrix.CreateTranslation(new Vector3(-_position * parallax, 0.0f)) *
                // The next line has a catch. See note below.
                Matrix.CreateTranslation(new Vector3(-_origin, 0.0f)) *
                Matrix.CreateRotationZ(_rotation) *
                Matrix.CreateScale(Zoom, _zoom, 1) *
                Matrix.CreateTranslation(new Vector3(_origin, 0.0f));
        }

        // When moving the camera around while it’s rotated, how do I make it follow the camera’s rotation?
        // The trick is to rotate the displacement vector by the inverse of the camera’s rotation.
        // This is done by negating the rotation angle.
        // So, if the camera is rotated 45 degrees, and I want to move it 10 pixels to the right,
        // I need to rotate that 10-pixel vector by -45 degrees.
        // This is done in the Move method.
        // The camera’s rotation is in radians, so you can use the Matrix.CreateRotationZ method to create a rotation matrix.
        // Then, you can use the Vector2.Transform method to rotate the displacement vector by that matrix.
        // This will give you the correct displacement vector to move the camera in the direction it’s facing.
        // The final step is to add the rotated displacement vector to the camera’s position.
        // This will move the camera in the direction it’s facing, while still allowing you to move it in the world space.
        // This is done in the Move method.
        public void Move(Vector2 displacement, bool respectRotation = false)
        {
            if (respectRotation)
            {
                displacement = Vector2.Transform(displacement, Matrix.CreateRotationZ(-Rotation));
            }

            _position += displacement;
        }
        // This method is used to look at a specific position in the world.
        // It sets the camera’s position to the specified position minus half the viewport size.
        // This centers the camera on the specified position.
        // The viewport size is divided by 2.0f to get the center of the viewport.
        // The position is a Vector2, so we subtract the viewport size from the position.
        // This will move the camera to the specified position, while keeping it centered on the viewport.
        // The viewPort parameter is the viewport of the game.
        // The viewport is used to get the size of the viewport.

        // Updates your camera to lock on the character
        // camera.LookAt(character.Position);
        public void LookAt(Vector2 position, Viewport viewPort)
        {
            _position = position - new Vector2(viewPort.Width / 2.0f, viewPort.Height / 2.0f);
        }
        public void LookAt(Vector2 position)
        {
            _position = position - new Vector2(_viewPort.Width / 2.0f, _viewPort.Height / 2.0f);
        }



    }

}
