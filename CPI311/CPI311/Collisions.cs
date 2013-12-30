using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Common;

namespace CPI311
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Collisions : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;        // Drawing 2D stuff
        SpriteFont textFont;            // Object for a font
        BasicEffect effect;
        Model sphereModel;
        Texture2D sphereTexture;
        Effect simpleEffect;
        int elevationMethod = 0;
        int shaderMethod = 0;
        KeyboardState prevKeyboardState = Keyboard.GetState();
        MouseState prevMouseState = Mouse.GetState();
        
        Common.HeightMap plane;
        Camera camera;
        List<RigidObject> spheres = new List<RigidObject>();
        Random random = new Random();
        int[] collisions = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        int secondIndex = 0;
        int millisecondCount = 0;
        float averageCollisions = 0;

        RigidObject[] lights = new RigidObject[3];

        // Simulate two Triangles
        Vector3[] vertices = { new Vector3(-10, -10, -10), new Vector3(10, -10, -10), 
                                 new Vector3(10, -10, 10), new Vector3(-10, -10, 10),
                             new Vector3(-10, 10, -10), new Vector3(10, 10, -10), 
                             new Vector3(10, 10, 10), new Vector3(-10, 10, 10)};
        int[] indices = { 0, 1, 2, 0, 2, 3, // bottom
                          4, 6, 5, 4, 7, 6, // top
                          0, 5, 1, 0, 4, 5, // front
                          3, 2, 6, 3, 6, 7, // back
                          0, 3, 7, 0, 7, 4, // left
                          1, 6, 2, 1, 5, 6}; // right

        public Collisions()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            textFont = Content.Load<SpriteFont>("Fonts/SegoeUI");    // Load the font
            sphereModel = Content.Load<Model>("Models/Sphere");
            sphereTexture = Content.Load<Texture2D>("Textures/Stripes");
            
            // TODO: use this.Content to load your game content here
            camera = new Camera();
            camera.Position = new Vector3(0, 0, -20);
            camera.AspectRatio = GraphicsDevice.Viewport.AspectRatio;

            plane = new Common.HeightMap(Content.Load<Texture2D>("Textures/HeightMap"));
            plane.Texture = Content.Load<Texture2D>("Textures/Jellyfish");
            plane.Scale *= 50;
            plane.Position = new Vector3(0, -15, 0);

            effect = new BasicEffect(GraphicsDevice);
            simpleEffect = Content.Load<Effect>("Effects/SimpleEffect");

            AddSphere();
            AddSphere();
            AddSphere();

            ChooseRandomLights();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            // Update both the spheres
            foreach (RigidObject sphere in spheres)
            {
                sphere.Update(gameTime);
                /*
                cells[(int)sphere.Position.X,
                        (int)sphere.Position.Y,
                        (int)sphere.Position.Z].Objects.Add(sphere);
                */
            }
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.W))
                camera.Position += camera.Forward * gameTime.ElapsedGameTime.Milliseconds / 1000f;
            if (keyboardState.IsKeyDown(Keys.S))
                camera.Position -= camera.Forward * gameTime.ElapsedGameTime.Milliseconds / 1000f;
            if (keyboardState.IsKeyDown(Keys.Left))
                camera.RotateY = gameTime.ElapsedGameTime.Milliseconds / 1000f;
            if (keyboardState.IsKeyDown(Keys.Right))
                camera.RotateY = -gameTime.ElapsedGameTime.Milliseconds / 1000f;
            if (keyboardState.IsKeyDown(Keys.Up))
                camera.RotateX = gameTime.ElapsedGameTime.Milliseconds / 1000f;
            if (keyboardState.IsKeyDown(Keys.Down))
                camera.RotateX = -gameTime.ElapsedGameTime.Milliseconds / 1000f;
            if (keyboardState.IsKeyDown(Keys.Tab) && prevKeyboardState.IsKeyUp(Keys.Tab))
                elevationMethod = (elevationMethod + 1) % 2;
            if (keyboardState.IsKeyDown(Keys.Enter) && prevKeyboardState.IsKeyUp(Keys.Enter))
                AddSphere();
            if (keyboardState.IsKeyDown(Keys.LeftControl) && prevKeyboardState.IsKeyUp(Keys.LeftControl))
            {
                shaderMethod = (shaderMethod + 1) % 2;
                simpleEffect.CurrentTechnique = simpleEffect.Techniques[shaderMethod];
            }
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                float xdiff = mouseState.X - prevMouseState.X;
                float ydiff = mouseState.Y - prevMouseState.Y;
                camera.RotateY = -xdiff / 100;
                camera.RotateX = ydiff / 100;
            }

            Vector3 position = new Vector3(MathHelper.Clamp(camera.Position.X, -50, 50),0,
                                        MathHelper.Clamp(camera.Position.Z, -50, 50));
            Vector2 tex = new Vector2((position.X + 50) / 100f, (position.Z + 50) / 100f);
            switch (elevationMethod)
            {
                case 0: position.Y = plane.GetElevationBilinear(tex) * plane.Scale.Y + 1; break;
                case 1: position.Y = plane.GetElevationNearest(tex) * plane.Scale.Y + 1; break;
            }
            camera.Position = position;

            // Cheap test for collision against walls
            // We will do better!
            /*
            for (int i = 0; i < 2; i++)
            {
                if (spheres[i].Position.Y < plane.Position.Y &&
                    Vector3.Dot(spheres[i].Velocity, Vector3.Up) < 0)
                    spheres[i].AddForce = spheres[i].Momentum * Vector3.Normalize(-spheres[i].Velocity) * 2;

                if (spheres[i].Position.Y > 10 &&
                    Vector3.Dot(spheres[i].Velocity, Vector3.Down) < 0)
                    spheres[i].AddForce = spheres[i].Momentum * Vector3.Normalize(-spheres[i].Velocity) * 2;
            }
            */
            // Do a worst case (all pairwise) collision detection
            for (int i = 0; i < spheres.Count; i++)
            {
                for (int j = 0; j < indices.Length / 3; j++)
                    TestCollision(spheres[i], vertices[indices[j * 3]], vertices[indices[j * 3 + 1]], vertices[indices[j * 3 + 2]]);
                for (int j = i + 1; j < spheres.Count; j++)
                    TestCollision(spheres[i], spheres[j]);
            }
            prevKeyboardState = keyboardState;
            prevMouseState = mouseState;

            // Average collisions per second
            millisecondCount += gameTime.ElapsedGameTime.Milliseconds;
            if (millisecondCount > 999)
            {
                millisecondCount -= 1000;
                secondIndex = (secondIndex + 1) % collisions.Length;
                averageCollisions = 0;
                foreach (int i in collisions)
                    averageCollisions += i;
                averageCollisions /= collisions.Length;
                collisions[secondIndex] = 0;
                ChooseRandomLights();
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Clear the screen
            GraphicsDevice.Clear(Color.Goldenrod);
            // Set the Depth Stencil State to default for 3D rendering
            GraphicsDevice.DepthStencilState = new DepthStencilState();
            // Setup some colors for lighting
            effect.EmissiveColor = new Vector3(0.2f, 0.2f, 0.2f);
            effect.DiffuseColor = new Vector3(0.5f, 0.0f, 0.0f);
            effect.SpecularColor = new Vector3(0.0f, 0.5f, 0.0f);
            effect.SpecularPower = 10;
            effect.LightingEnabled = true; // Enable lighting!
            effect.TextureEnabled = true; // Enable texturing
            // Some parameters for the Directional lighting
            effect.DirectionalLight0.Direction = new Vector3(0, -1, 1);
            effect.DirectionalLight0.SpecularColor = Vector3.One;
            // Provide the different matrices
            effect.View = camera.View;
            effect.Projection = camera.Projection;
            // -- for SimpleEffect
            simpleEffect.Parameters["View"].SetValue(camera.View);
            simpleEffect.Parameters["Projection"].SetValue(camera.Projection);
            simpleEffect.Parameters["CameraPosition"].SetValue(camera.Position);
            simpleEffect.Parameters["RedPosition"].SetValue(lights[0].Position);
            simpleEffect.Parameters["GreenPosition"].SetValue(lights[1].Position);
            simpleEffect.Parameters["BluePosition"].SetValue(lights[2].Position);
            
            // Cycle through all objects in the array
            foreach (RigidObject sphere in spheres)
            {
                simpleEffect.Parameters["World"].SetValue(sphere.World);
                simpleEffect.Parameters["BallColor"].SetValue(sphere.BallColor);
                effect.World = sphere.World;
                effect.Texture = sphere.Texture;
                sphere.Draw(simpleEffect);
            }
            simpleEffect.Parameters["World"].SetValue(plane.World);
            effect.World = plane.World;
            effect.Texture = plane.Texture;
            plane.Draw(simpleEffect);

            spriteBatch.Begin();    // First, start the sprite batch
            spriteBatch.DrawString(textFont, elevationMethod == 0? "Bilinear Interpolation" : "Nearest Neighbor", new Vector2(50, 50), Color.Black);
            spriteBatch.DrawString(textFont, "Average Collision: " + averageCollisions, new Vector2(50, 75), Color.Black);
            spriteBatch.DrawString(textFont, "Spheres: " + spheres.Count, new Vector2(50, 100), Color.Black);
            spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
        /// <summary>
        /// Sphere / Sphere Collision
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public void TestCollision(RigidObject first, RigidObject second)
        {
            // Simple sphere / sphere collision
            Vector3 direction = first.Position - second.Position;
            if (direction.LengthSquared() < Math.Pow(first.Scale.X + second.Scale.X, 2))
            {
                float diff = direction.Length() - first.Scale.X - second.Scale.X;
                // First, normalize the direction vector
                direction.Normalize();
                // If the spheres are overlapping, push them out a little so they just touch
                first.Position -= diff / 2 * direction;
                second.Position += diff / 2 * direction;
                // Next, find the relative velocity between the two bodies
                Vector3 relativeVelocity = first.Velocity - second.Velocity;
                // Find the velocity in the direction of contact - use dot product
                float relativeVelocityPerp = -Vector3.Dot(relativeVelocity, direction);
                // Esoteric: Finally, the impulse is determined using this relationship
                float impulse = relativeVelocityPerp  / (1 / first.Mass + 1 / second.Mass);
                // Add the impulses in the correct direction to both bodies
                first.AddForce = 2 * impulse * direction;
                second.AddForce = -2 * impulse * direction;
                collisions[secondIndex]++;
            }
        }

        /// <summary>
        /// Sphere / Triangle Collision
        /// </summary>
        /// <param name="first"></param>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        public void TestCollision(RigidObject first, Vector3 A, Vector3 B, Vector3 C)
        {
            // First, make the sphere center our origin
            A = A - first.Position;
            B = B - first.Position;
            C = C - first.Position;
            // Find the surface normal for the triangle (using a cross product)
            Vector3 normal = Vector3.Cross(C - A, B - A);
            normal.Normalize();
            // Find how far away the sphere is from the triangle
            // We use a dot prouct to "project" Vector A on the normal
            float separation = (float)Math.Abs(Vector3.Dot(A, normal));
            // If the sphere is too far away, then quit
            if (separation > first.Scale.X)
                return;
            // Good, so now it is in range! Find the contact point on the plane of the triangle
            Vector3 P = separation * normal;
            // Esoteric: Determine if the point is actually in the triangle
            if (Vector3.Dot(Vector3.Cross(B - A, P - A), Vector3.Cross(C - A, P - A)) > 0 ||
                Vector3.Dot(Vector3.Cross(B - C, P - C), Vector3.Cross(A - C, P - C)) > 0)
                return;
            // If the sphere is actually moving away, then don't bother (dot product here
            if (Vector3.Dot(first.Velocity, normal) > 0)
                return;
            // Finally, add the impulse force to the sphere
            first.AddForce = -2 * Vector3.Dot(first.Velocity, normal) * normal * first.Mass;
        }

        private void AddSphere()
        {
            RigidObject sphere = new RigidObject();
            sphere.Model = sphereModel;
            sphere.Texture = sphereTexture;
            sphere.Position = new Vector3(
                                (float)(random.NextDouble()*20 - 10),
                                (float)(random.NextDouble()*20 - 10),
                                (float)(random.NextDouble()*20 - 10));
            sphere.Velocity = new Vector3(
                                (float)(random.NextDouble()*4 - 2),
                                (float)(random.NextDouble()*4 - 2),
                                (float)(random.NextDouble()*4 - 2));
            sphere.Mass = (float)(1 + random.NextDouble() * 4);
            sphere.Scale *= (float)(0.5 + random.NextDouble());
            sphere.BallColor = RigidObject.ColorBank[random.Next(0, 6)];
            spheres.Add(sphere);
        }

        private void ChooseRandomLights()
        {
            lights[0] = spheres[random.Next(0, spheres.Count)];
            lights[1] = spheres[random.Next(0, spheres.Count)];
            lights[2] = spheres[random.Next(0, spheres.Count)];
        }

    }
}
