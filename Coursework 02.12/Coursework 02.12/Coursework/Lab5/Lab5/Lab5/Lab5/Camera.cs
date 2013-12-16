using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Lab5
{
    class Camera : GameComponent
    {
        //Attributes
        private Vector3 cameraPosition;
        private Vector3 cameraRotation;
        private float cameraSpeed;
        private Vector3 cameraLookAt;
        private Vector3 mouseRotationBuffer;
        private MouseState currentMouseState;
        private MouseState prevMouseState;


        //Properties

        public Vector3 Position
        {
            get { return cameraPosition; }
            set
            {
                cameraPosition = value;
                UpdateLookAt();
            }
        }

        public Vector3 Rotation
        {
            get { return cameraRotation; }
            set
            {
                cameraRotation = value;
                UpdateLookAt();
            }
        }

        public Matrix Projection
        {
            get;
            protected set;
        }

        public Matrix View
        {
            get
            {
                return Matrix.CreateLookAt(cameraPosition, cameraLookAt, Vector3.Up);
            }
        }

        //Constructor
        public Camera(Game game, Vector3 position, Vector3 rotation, float speed)
            : base(game)
        {
            cameraSpeed = speed;

            //Setup projection matrix
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                Game.GraphicsDevice.Viewport.AspectRatio,
                0.05f,
                3000.0f);

            //Set camera position and rotation
            MoveTo(position, rotation);



            prevMouseState = Mouse.GetState();
        }

        //Set camera's position and rotation
        private void MoveTo(Vector3 pos, Vector3 rot)
        {
            Position = pos;
            Rotation = rot;
        }

        //update the look at vector
        private void UpdateLookAt()
        {
            //Build a rotation matrix
            Matrix rotationMatrix = Matrix.CreateRotationX(cameraRotation.X) * Matrix.CreateRotationY(cameraRotation.Y);
            //Build look at offset vector
            Vector3 lookAtOffset = Vector3.Transform(Vector3.UnitZ, rotationMatrix);
            //Update our camera's look at vector
            cameraLookAt = cameraPosition + lookAtOffset;
        }

        //Method that simulates movement
        private Vector3 PreviewMove(Vector3 amount)
        {
            //Create a rotate matrix
            Matrix rotate = Matrix.CreateRotationY(cameraRotation.Y);
            //Create a movement vector
            Vector3 movement = new Vector3(amount.X, amount.Y, amount.Z);
            movement = Vector3.Transform(movement, rotate);
            //Return the value of camera position + movement vector
            return cameraPosition + movement;
        }

        //Method that actually moves the camera
        private void Move(Vector3 scale)
        {
            MoveTo(PreviewMove(scale), Rotation);
        }

        //update method
        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            currentMouseState = Mouse.GetState();

            KeyboardState ks = Keyboard.GetState();

            //Handle basic key movement

            Vector3 moveVector = Vector3.Zero;

            if (ks.IsKeyDown(Keys.W))
                moveVector.Z = 2;
            if (ks.IsKeyDown(Keys.S))
                moveVector.Z = -2;
            if (ks.IsKeyDown(Keys.A))
                moveVector.X = 2;
            if (ks.IsKeyDown(Keys.D))
                moveVector.X = -2;

            if (moveVector != Vector3.Zero)
            {
                //normalize that vector
                //so that we don't move faster diagonally
                moveVector.Normalize();
                //Now we add in smooth and speed
                moveVector *= dt * cameraSpeed * 2;

                //Move camera
                Move(moveVector);
            }

            //Handle mouse movement

            float deltaX;
            float deltaY;

            
            if (currentMouseState != prevMouseState)
            {
                //Cache mouse location
                deltaX = currentMouseState.X - (Game.GraphicsDevice.Viewport.Width / 2);
                deltaY = currentMouseState.Y - (Game.GraphicsDevice.Viewport.Height / 2);

                

                //Calculate rotation from mouse movement
                mouseRotationBuffer.X -= 0.1f * deltaX * dt;
                mouseRotationBuffer.Y -= 0.1f * deltaY * dt;

                //Clamp the rotational movement
                if (mouseRotationBuffer.Y < MathHelper.ToRadians(-75.0f))
                    mouseRotationBuffer.Y = mouseRotationBuffer.Y - (mouseRotationBuffer.Y - MathHelper.ToRadians(-75.0f));
                if (mouseRotationBuffer.Y > MathHelper.ToRadians(75.0f))
                    mouseRotationBuffer.Y = mouseRotationBuffer.Y - (mouseRotationBuffer.Y - MathHelper.ToRadians(75.0f));

                //Finally add that rotation to our rotation vector clamping as needed
                Rotation = new Vector3(-MathHelper.Clamp(mouseRotationBuffer.Y,
                                    MathHelper.ToRadians(-75.0f), MathHelper.ToRadians(75.0f)),
                                    MathHelper.WrapAngle(mouseRotationBuffer.X), 0);

                //Reset our change in mouse position
                deltaX = 0;
                deltaY = 0;

            }
            

            //Set mouse cursor to center of screen
            Mouse.SetPosition(Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2);

            //Set prev state to current state
            prevMouseState = currentMouseState;

            base.Update(gameTime);
        }

    }
}
