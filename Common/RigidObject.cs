using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Common
{
    public class RigidObject : ModelObject
    {
        public static Vector4[] ColorBank = 
        {
            Color.Red.ToVector4(),
            Color.Green.ToVector4(),
            Color.Blue.ToVector4(),
            Color.Yellow.ToVector4(),
            Color.Cyan.ToVector4(),
            Color.Magenta.ToVector4(),
        };

        public Vector3 Velocity { get; set; }
        public float Mass { get; set; }
        public float Friction { get; set; }
        public Vector3 Gravity { get; set; }
        public Vector4 BallColor { get; set; }

        public Vector3 Force { get; set; }
        public Vector3 AddForce
        {
            set
            {
                Force += value;
            }
        }

        public float Momentum
        {
            get { return Mass * Velocity.Length(); }
        }

        public RigidObject()
            : base()
        {
            Mass = 1;
            Friction = 0;
            Velocity = Vector3.Zero;
            Force = Vector3.Zero;
        }

        public void Update(GameTime gameTime)
        {
            
            Velocity += Force / Mass + Gravity / Mass * gameTime.ElapsedGameTime.Milliseconds / 1000f;
            Force = Vector3.Zero;
            Position += Velocity * gameTime.ElapsedGameTime.Milliseconds / 1000f;
        }
    }
}
