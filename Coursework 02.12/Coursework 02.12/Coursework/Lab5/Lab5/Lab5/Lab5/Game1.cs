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
        private Song bkgMusic;
        private String songInfo;
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
        private Vector3 mdlPosition = new Vector3(11.0f, 1.0f, 35.0f);
        private float mdlRotation = -65.0f;
        private Vector3 mdlVelocity = Vector3.Zero;

        // create an array of enemy daleks
        private Model mdlDalek;
        private Matrix[] mdDalekTransforms;
        private Daleks[] dalekList = new Daleks[GameConstants.NumDaleks];

        //Create Boss
        private Model mdlBoss;
        private Matrix[] mdlBossTransforms;
        private Daleks[] gameBoss = new Daleks[GameConstants.bossNum];

        // create an array of laser bullets
        private Model mdlLaser;
        private Matrix[] mdlLaserTransforms;
        private Laser[] laserList = new Laser[GameConstants.NumLasers];

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
        bool myCam = true;


        private void InitializeTransform()
        {
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;


                camera = new Camera(this, new Vector3(10.0f, 1.0f, 1.0f), Vector3.Zero, 5.0f);
                Components.Add(camera);
                camTwo = new Camera(this, new Vector3(10.0f, 5.0f, -10.0f), new Vector3(0.3f, 0.2f, 0.0f), 5.0f);
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

           /* if (keyboardState.IsKeyDown(Keys.Left))
            {
                // Rotate left.
                mdlRotation -= -1.0f * 0.10f;
            }

            if (keyboardState.IsKeyDown(Keys.Right))
            {
                // Rotate right.
                mdlRotation -= 1.0f * 0.10f;
                
            }*/

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
                mdlRotation = -65.0f;
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
                        Matrix tardisTransform = Matrix.CreateRotationY(mdlRotation);
                        laserList[i].direction = tardisTransform.Forward;
                        laserList[i].speed = GameConstants.LaserSpeedAdjustment;
                        laserList[i].position = mdlPosition + laserList[i].direction;
                        laserList[i].isActive = true;
                        //firingSound.Play();
                        break; //exit the loop     
                    }
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
            float xStart;
            float zStart;
            for (int i = 0; i < GameConstants.NumDaleks; i++)
            {
               /* if (random.Next(2) == 0)
                {
                    xStart = (float)-GameConstants.PlayfieldSizeX;
                }
                else
                {
                    xStart = (float)GameConstants.PlayfieldSizeX;
                }*/
                //zStart = (float)random.NextDouble() * GameConstants.PlayfieldSizeZ;

                dalekList[i].position = new Vector3(15.0f, 1.0f, 35.0f);
                double angle = random.NextDouble() * 2 * Math.PI;
                //dalekList[i].direction.X = -(float)Math.Sin(angle);
                dalekList[i].direction.X = 1;

                //dalekList[i].direction.Y = (float)Math.Tan(angle);
                dalekList[i].speed = (GameConstants.DalekMinSpeed +
                   (float)random.NextDouble() * GameConstants.DalekMaxSpeed) / 5;
                dalekList[i].isActive = true;
            }

            gameBoss[0].position = new Vector3(1.0f, 1.0f, 10.0f);
            gameBoss[0].isActive = true;


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
                        //effect.EnableDefaultLighting();
                        //effect.AmbientLightColor = new Vector3(1.0f, 1.0f, 1.0f);
                        //effect.DirectionalLight0.Enabled = true;
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
                    effect.AmbientLightColor = new Vector3(1.0f, 1.0f, 1.0f);
                    effect.DirectionalLight0.Enabled = true;
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

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            this.IsMouseVisible = true;
            Window.Title = "Lab 6 - Collision Detection";
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
            bkgMusic = Content.Load<Song>(".\\Audio\\DoctorWhotheme11");
            //MediaPlayer.Play(bkgMusic);
            //MediaPlayer.IsRepeating = true;
            songInfo = "Song: " + bkgMusic.Name + " Song Duration: " + bkgMusic.Duration.Minutes + ":" + bkgMusic.Duration.Seconds;
            //-------------------------------------------------------------
            // added to load Model
            //-------------------------------------------------------------
            mdlTardis = Content.Load<Model>(".\\Models\\YellowSubmarine");
            mdlTardisTransforms = SetupEffectTransformDefaults(mdlTardis);
            mdlDalek = Content.Load<Model>(".\\Models\\dalek");
            mdDalekTransforms = SetupEffectTransformDefaults(mdlDalek);
            mdlLaser = Content.Load<Model>(".\\Models\\laser");
            mdlLaserTransforms = SetupEffectTransformDefaults(mdlLaser);
            mdlBoss = Content.Load<Model>(".\\Models\\toaster");
            mdlBossTransforms = SetupEffectTransformDefaults(mdlBoss);

            skyBox = Content.Load<Model>(".\\SkyBox\\skybox2");
            sbTransform = SetupEffectTransformDefaults(skyBox);
            //-------------------------------------------------------------
            // added to load SoundFX's
            //-------------------------------------------------------------
            tardisSound = Content.Load<SoundEffect>("Audio\\tardisEdit");
            explosionSound = Content.Load<SoundEffect>("Audio\\explosion2");
            firingSound = Content.Load<SoundEffect>("Audio\\shot007");
            tardisSoundInstance = tardisSound.CreateInstance();
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
                dalekList[i].Update(timeDelta);
            }

            for (int i = 0; i < GameConstants.NumLasers; i++)
            {
                if (laserList[i].isActive)
                {
                    laserList[i].Update(timeDelta);
                }
            }

            gameBoss[0].Update(timeDelta);

            BoundingSphere TardisSphere =
              new BoundingSphere(mdlPosition,
                       mdlTardis.Meshes[0].BoundingSphere.Radius *
                             GameConstants.ShipBoundingSphereScale);

            //Check for collisions
            for (int i = 0; i < dalekList.Length; i++)
            {
                if (dalekList[i].isActive)
                {
                    BoundingSphere dalekSphereA =
                      new BoundingSphere(dalekList[i].position, mdlDalek.Meshes[0].BoundingSphere.Radius *
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
                                dalekList[i].isActive = false;
                                laserList[k].isActive = false;
                                hitCount++;
                                break; //no need to check other bullets
                            }
                        }
                        if (dalekSphereA.Intersects(TardisSphere)) //Check collision between Dalek and Tardis
                        {
                            //explosionSound.Play();
                            dalekList[i].direction *= -1.0f;
                            //laserList[k].isActive = false;
                            break; //no need to check other bullets
                        }

                    }
                }
            }
            base.Update(gameTime);
        }

       

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Matrix sbmodelTransform = Matrix.CreateRotationY(38) * Matrix.CreateTranslation(sbPos) * Matrix.CreateScale(30.0f);
            DrawModel(skyBox, sbmodelTransform, sbTransform);

            // TODO: Add your drawing code here
            for (int i = 0; i < GameConstants.NumDaleks; i++)
            {
                if (dalekList[i].isActive)
                {
                    Matrix dalekTransform = Matrix.CreateScale(GameConstants.DalekScalar) * Matrix.CreateTranslation(dalekList[i].position);
                    DrawModel(mdlDalek, dalekTransform, mdDalekTransforms);
                }
            }

            if (gameBoss[0].isActive)
            {
                Matrix bossTransform = Matrix.CreateScale(GameConstants.BossScalar) * Matrix.CreateTranslation(gameBoss[0].position);
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

            Matrix modelTransform = Matrix.CreateRotationY(mdlRotation) * Matrix.CreateTranslation(mdlPosition);
            DrawModel(mdlTardis, modelTransform, mdlTardisTransforms);

            writeText("Tardis Vs Daleks", new Vector2(50, 10), Color.Yellow);
            writeText("Instructions\nPress The Arrow keys to move the Tardis\nSpacebar to fire!\nR to Reset", new Vector2(50, 50), Color.Black);

            writeText(songInfo, new Vector2(50, 125), Color.AntiqueWhite);

            base.Draw(gameTime);
        }
    }
}



