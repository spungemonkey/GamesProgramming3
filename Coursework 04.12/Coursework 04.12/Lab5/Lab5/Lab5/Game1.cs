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
using System.Diagnostics;


namespace Lab5
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        #region User Defined Variables
        //------------------------------------------
        // Added for use with fonts
        //------------------------------------------
        SpriteFont fontToUse;

        //--------------------------------------------------
        // Added for use with playing Audio via Media player
        //--------------------------------------------------
        private Song gameTheme;
        private Song bossTheme;
        private Song endTheme;

        //--------------------------------------------------
        //Set the sound effects to use
        //--------------------------------------------------
        private SoundEffectInstance tardisSoundInstance;
        private SoundEffect tardisSound;
        private SoundEffect explosionSound;
        private SoundEffect firingSound;

        // Set the 3D model to draw.
        private Model mdlTardis;
        private Matrix[] mdlTardisTransforms;

        // The aspect ratio determines how to scale 3d to 2d projection.
        private float aspectRatio;

        // Set the position of the model in world space, and set the rotation.
        //private Vector3 mdlPosition = Vector3.Zero;
        private Vector3 mdlPosition = new Vector3(35.0f, 1.0f, 35.0f);
        private float mdlRotation = 0.1f;
        private Vector3 mdlVelocity = Vector3.Zero;

        // create an array of Dreadful Flying Gloves
        private Model mdlDFG;
        private Matrix[] mdDFGTramsforms;
        private Daleks[] dfgList = new Daleks[GameConstants.NumDaleks];

        //Create Boss
        private Model mdlBoss;
        private Matrix[] mdlBossTransforms;
        private Vector3 bossPos = new Vector3 (-30.0f, -5.0f, 35.0f);
        private bool gameBoss = false;

        // create an array of laser bullets
        private Model mdlLaser;
        private Matrix[] mdlLaserTransforms;
        private Laser[] laserList = new Laser[GameConstants.NumLasers];


                private Vector3[] bossPoints = {
                                           new Vector3(-15.0f, 3.0f, 0.0f), 
                                           new Vector3(-10.0f, 13.0f, 0.0f),
                                           new Vector3(-12.5f, 19.0f, 0.0f),
                                           new Vector3(-13.5f, 7.0f, 0.0f),
                                           new Vector3(-9.5f, 0.0f, 0.0f),
                                           new Vector3(-18.5f, -6.0f, 0.0f),
                                           new Vector3(-16.5f, -3.0f, 0.0f),
                                           new Vector3(-12.5f, -12.0f, 0.0f),
                                       };

                private Laser[] waypoints = new Laser[8];
                private Model mdlWaypoints;
                private Matrix[] mdlWaypointTransforms;

        //setup skybox
        private Model skyBox;
        private Matrix[] sbTransform;
        private Vector3 sbPos = Vector3.Zero;


        private Random random = new Random();

        private KeyboardState lastState;
        private int hitCount;

        // Set the position of the camera in world space, for our view matrix.
        Camera camera;
        Camera camTwo;
        bool myCam = false;

        //player attributes
        int pHealth = 100;
        
        int count = 0;
        int totalCount = 0;


        private void InitializeTransform()
        {
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            camera = new Camera(this, new Vector3(10.0f, 1.0f, 1.0f), Vector3.Zero, 5.0f);
            Components.Add(camera);
            camTwo = new Camera(this, new Vector3(10.0f, 0.0f, -10.0f), Vector3.Zero, 5.0f);
            Components.Add(camTwo);
        }

        private void MoveModel()
        {
            KeyboardState keyboardState = Keyboard.GetState();

            // Create some velocity if the right trigger is down.
            Vector3 mdlVelocityAdd = Vector3.Zero;

            // Find out what direction we should be thrusting, using rotation.
            mdlVelocityAdd.X = -(float)Math.Sin(0);
            mdlVelocityAdd.Y = -(float)Math.Tan(1);



            if (keyboardState.IsKeyDown(Keys.Down))
            {
                // Rotate left.
                // Create some velocity if the right trigger is down.
                // Now scale our direction by how hard the trigger is down.
                mdlVelocityAdd *= 0.01f;
                mdlVelocity += mdlVelocityAdd;

                if (mdlPosition.Y <= GameConstants.PlayfieldminY)
                {
                    mdlVelocityAdd *= 0.0f;
                    mdlVelocity *= mdlVelocityAdd;
                }

            }

            if (keyboardState.IsKeyDown(Keys.Up))
            {
                // Rotate left.
                // Now scale our direction by how hard the trigger is down.
                mdlVelocityAdd *= -0.01f;
                mdlVelocity += mdlVelocityAdd;

                if (mdlPosition.Y >= GameConstants.PlayfieldmaxY)
                {
                    mdlVelocityAdd *= 0.0f;
                    mdlVelocity *= mdlVelocityAdd;
                }

            }

            if (keyboardState.IsKeyDown(Keys.R))
            {
                mdlVelocity = Vector3.Zero;
                mdlPosition = new Vector3(11.0f, 1.0f, 35.0f);
                mdlRotation = 0.0f;
                myCam = false;
                //tardisSoundInstance.Play();
            }

            //are we shooting?
            if (keyboardState.IsKeyDown(Keys.Space) || lastState.IsKeyDown(Keys.Space))
            {
                //add another bullet.  Find an inactive bullet slot and use it
                //if all bullets slots are used, ignore the user input
                for (int i = 0; i < GameConstants.NumLasers; i++)
                {
                    if (!laserList[i].isActive)
                    {
                        Matrix tardisTransform = Matrix.CreateRotationX(90);
                        laserList[i].direction = tardisTransform.Left;
                        laserList[i].speed = GameConstants.LaserSpeedAdjustment;
                        laserList[i].position = mdlPosition + laserList[i].direction;
                        laserList[i].isActive = true;
                        //firingSound.Play();
                        break; //exit the loop     
                    }
                }
            }

            if (keyboardState.IsKeyDown(Keys.D1))
            {
                myCam = true;
                camera.Position = new Vector3(10.0f, 1.0f, 1.0f);

            }
            if (keyboardState.IsKeyDown(Keys.D2))
            {
                myCam = false;
                camTwo.Position = new Vector3(10.0f, 0.0f, -10.0f);
            }

            if (myCam == false)
            {
                if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.D))
                {
                    camTwo.Position = new Vector3(10.0f, 1.0f, 1.0f);
                }
            }

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            lastState = keyboardState;

        }

        private void ResetDaleks()
        {
            Matrix dfgRot = new Matrix();
            int y;
            for (int i = 0; i < GameConstants.NumDaleks; i++)
            {

                dfgList[i].position = new Vector3((-30.0f - (i*2)), (1.0f * (i*5)), 35.0f);
                double angle = random.NextDouble() * 2 * Math.PI;
                dfgList[i].direction.X = 1;

                dfgList[i].speed = (GameConstants.DalekMinSpeed + 0.21f * GameConstants.DalekMaxSpeed) / 5;

                dfgList[i].rotation = dfgRot.Left;
                dfgList[i].isActive = true;
            }
        }

        private Matrix[] SetupEffectTransformDefaults(Model myModel)
        {
            Matrix[] absoluteTransforms = new Matrix[myModel.Bones.Count];
            myModel.CopyAbsoluteBoneTransformsTo(absoluteTransforms);

            foreach (ModelMesh mesh in myModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();

                    if (myCam)
                    {
                        effect.Projection = camera.Projection;
                        effect.View = camera.View;

                    }
                    else
                    {
                        effect.Projection = camTwo.Projection;
                        effect.View = camTwo.View;
                    }
                }
            }
            return absoluteTransforms;
        }

        public void DrawModel(Model model, Matrix modelTransform, Matrix[] absoluteBoneTransforms)
        {
            //Draw the model, a model can have multiple meshes, so loop
            foreach (ModelMesh mesh in model.Meshes)
            {
                //This is where the mesh orientation is set
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = absoluteBoneTransforms[mesh.ParentBone.Index] * modelTransform;
                    effect.EnableDefaultLighting();

                    if (myCam)
                    {
                        effect.Projection = camera.Projection;
                        effect.View = camera.View;
                    }
                    else
                    {
                        effect.Projection = camTwo.Projection;
                        effect.View = camTwo.View;
                    }
                }
                //Draw the mesh, will use the effects set above.
                mesh.Draw();
            }
        }

        private void writeText(string msg, Vector2 msgPos, Color msgColour)
        {
            spriteBatch.Begin();
            string output = msg;
            // Find the center of the string
            Vector2 FontOrigin = fontToUse.MeasureString(output) / 2;
            Vector2 FontPos = msgPos;
            // Draw the string
            spriteBatch.DrawString(fontToUse, output, FontPos, msgColour);
            spriteBatch.End();
        }

        #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
           // this.IsMouseVisible = true;
        }

        BoundingSphere bossSphere = new BoundingSphere();

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            this.IsMouseVisible = false;
            Window.Title = "Blue Meanies v The Yellow Submarine";
            //**//
            hitCount = 0;
            InitializeTransform();
            ResetDaleks();


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
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            //-------------------------------------------------------------
            // added to load font
            //-------------------------------------------------------------
            fontToUse = Content.Load<SpriteFont>(".\\Fonts\\DrWho");
            //-------------------------------------------------------------
            // added to load Song
            //-------------------------------------------------------------
            gameTheme = Content.Load<Song>(".\\Audio\\YSIP");
            bossTheme = Content.Load<Song>(".\\Audio\\MOTM");
            MediaPlayer.Play(gameTheme);
            MediaPlayer.IsRepeating = true;

            if (gameBoss)
            {
                MediaPlayer.Stop();
                MediaPlayer.Play(bossTheme);
                MediaPlayer.IsRepeating = true;
            }

            //-------------------------------------------------------------
            // added to load Model
            //-------------------------------------------------------------
            mdlTardis = Content.Load<Model>(".\\Models\\YellowSubmarine");
            mdlTardisTransforms = SetupEffectTransformDefaults(mdlTardis);
            mdlDFG = Content.Load<Model>(".\\Models\\fister");
            mdDFGTramsforms = SetupEffectTransformDefaults(mdlDFG);
            mdlLaser = Content.Load<Model>(".\\Models\\laser");
            mdlLaserTransforms = SetupEffectTransformDefaults(mdlLaser);
            mdlBoss = Content.Load<Model>(".\\Models\\toaster");
            mdlBossTransforms = SetupEffectTransformDefaults(mdlBoss);

            skyBox = Content.Load<Model>(".\\SkyBox\\skybox2");
            sbTransform = SetupEffectTransformDefaults(skyBox);

            mdlWaypoints = Content.Load<Model>(".\\Models\\laser");
            mdlWaypointTransforms = SetupEffectTransformDefaults(mdlWaypoints);
            
            //-------------------------------------------------------------
            // added to load SoundFX's
            //-------------------------------------------------------------
            //tardisSound = Content.Load<SoundEffect>("Audio\\tardisEdit");
            //explosionSound = Content.Load<SoundEffect>("Audio\\explosion2");
            //firingSound = Content.Load<SoundEffect>("Audio\\shot007");
            //tardisSoundInstance = tardisSound.CreateInstance();
            //tardisSoundInstance.Play();


            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        protected override void Update(GameTime gameTime)
        {
            float posSpeed = GameConstants.posSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            float negSpeed = GameConstants.negSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            MoveModel();

            // Add velocity to the current position.
            mdlPosition += mdlVelocity;

            // Bleed off velocity over time.
            mdlVelocity *= 0.95f;

            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            for (int i = 0; i < GameConstants.NumDaleks; i++)
            {
                dfgList[i].Update(timeDelta);
            }

            for (int i = 0; i < GameConstants.NumLasers; i++)
            {
                if (laserList[i].isActive)
                {
                    laserList[i].Update(timeDelta);
                }
            }

            BoundingSphere TardisSphere =
              new BoundingSphere(mdlPosition,
                       mdlTardis.Meshes[0].BoundingSphere.Radius *
                             GameConstants.ShipBoundingSphereScale);


            


            //Check for collisions
            for (int i = 0; i < dfgList.Length; i++)
            {
                if (dfgList[i].isActive)
                {
                    BoundingSphere dalekSphereA =
                      new BoundingSphere(dfgList[i].position, mdlDFG.Meshes[0].BoundingSphere.Radius *
                                     GameConstants.DalekBoundingSphereScale);

                    for (int k = 0; k < laserList.Length; k++)
                    {
                        if (laserList[k].isActive)
                        {
                            BoundingSphere laserSphere = new BoundingSphere(
                              laserList[k].position, mdlLaser.Meshes[0].BoundingSphere.Radius *
                                     GameConstants.LaserBoundingSphereScale);
                            if (dalekSphereA.Intersects(laserSphere))
                            {
                                //explosionSound.Play();
                                dfgList[i].isActive = false;
                                laserList[k].isActive = false;
                                count++;
                                totalCount++;
                                break; //no need to check other bullets
                            }
                        }

                        if (dalekSphereA.Intersects(TardisSphere)) //Check collision between Dalek and Tardis
                        {
                            //explosionSound.Play();
                            dfgList[i].direction *= -1.0f;
                            dfgList[i].isActive = false;
                            pHealth--;
                            count++;
                            totalCount = totalCount + 1;
                            //laserList[k].isActive = false;
                            break; //no need to check other bullets
                        }

                    }
                }
            }


            if (count == 10)
            {
                ResetDaleks();
                count = 0;
            }

            if (totalCount >= dfgList.Length * 2)
            {
                    gameBoss = true;

            }


            if (pHealth == 0)
                this.Exit();

           // moveBoss(timeDelta);
            //moveBoss();





            float speed = 8.0f * timeDelta;
            int n = 0;

            bossSphere = new BoundingSphere(bossPos, mdlBoss.Meshes[0].BoundingSphere.Radius * 0.8f);

            waypoints[n].position = bossPoints[n];
            waypoints[n].isActive = true;

            BoundingSphere wpSphere = new BoundingSphere(waypoints[n].position, 0.46f);//mdlLaser.Meshes[0].BoundingSphere.Radius *
            //GameConstants.LaserBoundingSphereScale);

            if (gameBoss)
            {
                // for (int n = 0; n < bossPoints.Length; n++)
                //{

                if (bossSphere.Intersects(wpSphere))// && n < bossPoints.Length)
                {
                    n++;
                }
                if (n == bossPoints.Length)
                {
                    n = 0;
                }

                if (bossPos.X < bossPoints[n].X)
                {
                    bossPos.X += speed;
                }
                if (bossPos.X > bossPoints[n].X)
                {
                    bossPos.X -= speed;
                }
                if (bossPos.Y < bossPoints[n].Y)
                {
                    bossPos.Y += speed;
                }
                if (bossPos.Y > bossPoints[n].Y)
                {
                    bossPos.Y -= speed;
                }

            }







            base.Update(gameTime);
        }


        private void moveBoss(float delta)
        {           
            //float speed = 8.0f * delta;
            //int n = 0;

            //bossSphere = new BoundingSphere(bossPos, mdlBoss.Meshes[0].BoundingSphere.Radius * 0.8f);

            //waypoints[n].position = bossPoints[n];
            //waypoints[n].isActive = true;

            //BoundingSphere wpSphere = new BoundingSphere(waypoints[n].position, 0.46f);//mdlLaser.Meshes[0].BoundingSphere.Radius *
            //                         //GameConstants.LaserBoundingSphereScale);

            //if (gameBoss)
            //{
            //   // for (int n = 0; n < bossPoints.Length; n++)
            //    //{

            //   if (bossSphere.Intersects(wpSphere))// && n < bossPoints.Length)
            //    {
            //        n++;
            //    }
            //   if (n == bossPoints.Length)
            //   {
            //       n = 0;
            //   }

            //        if (bossPos.X < bossPoints[n].X)
            //        {
            //            bossPos.X += speed;
            //        }
            //        if (bossPos.X > bossPoints[n].X)
            //        {
            //            bossPos.X -= speed;
            //        }
            //        if (bossPos.Y < bossPoints[n].Y)
            //        {
            //            bossPos.Y += speed;
            //        }
            //        if (bossPos.Y > bossPoints[n].Y)
            //        {
            //            bossPos.Y -= speed;
            //        }

            //    }




        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Matrix sbmodelTransform = Matrix.CreateRotationY(38) * Matrix.CreateTranslation(sbPos) * Matrix.CreateScale(30.0f);
            DrawModel(skyBox, sbmodelTransform, sbTransform);

            // TODO: Add your drawing code here
            for (int i = 0; i < GameConstants.NumDaleks; i++)
            {
                if (dfgList[i].isActive)
                {
                    Matrix dalekTransform = Matrix.CreateRotationY(20) * Matrix.CreateScale(GameConstants.DalekScalar) * Matrix.CreateTranslation(dfgList[i].position);
                    DrawModel(mdlDFG, dalekTransform, mdDFGTramsforms);
                }
            }

            if (gameBoss)
            {
                Matrix bossTransform = Matrix.CreateScale(GameConstants.BossScalar) * Matrix.CreateTranslation(bossPos);
                DrawModel(mdlBoss, bossTransform, mdlBossTransforms);
            }

            for (int i = 0; i < GameConstants.NumLasers; i++)
            {
                if (laserList[i].isActive)
                {
                    Matrix laserTransform = Matrix.CreateScale(GameConstants.LaserScalar) * Matrix.CreateTranslation(laserList[i].position);
                    DrawModel(mdlLaser, laserTransform, mdlLaserTransforms);
                }
            }

            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i].isActive)
                {
                    Matrix wpTransform = Matrix.CreateScale(10.0f) * Matrix.CreateTranslation(waypoints[i].position);
                    DrawModel(mdlWaypoints, wpTransform, mdlWaypointTransforms);
                }
            }

            Matrix modelTransform = Matrix.CreateRotationY(mdlRotation) * Matrix.CreateTranslation(mdlPosition);
            DrawModel(mdlTardis, modelTransform, mdlTardisTransforms);

            writeText("Yellow Sub Vs Blue Meanies", new Vector2(50, 10), Color.Yellow);
            writeText(pHealth.ToString(), new Vector2(50, 50), Color.Yellow);
            writeText(totalCount.ToString(), new Vector2(50, 75), Color.Yellow);
            writeText(bossPos.X.ToString(), new Vector2(50, 100), Color.Yellow);
            writeText(bossPos.Y.ToString(), new Vector2(190, 100), Color.Yellow);

            base.Draw(gameTime);
        }
    }
}



