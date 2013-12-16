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

        private enum GameState
        {
            Menu,
            Play, 
            Boss, 
            Win,
            Lose,
        }
        GameState CurrentState = GameState.Play;

        //--------------------------------------------------
        //Set the sound effects to use
        //--------------------------------------------------
        private SoundEffect boltFire;
        private SoundEffect bossHit;

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
                                           new Vector3(-15.0f, 3.0f, 35.0f), 
                                           new Vector3(-10.0f, 13.0f, 35.0f),
                                           new Vector3(-12.5f, 19.0f, 35.0f),
                                           new Vector3(-13.5f, 7.0f, 35.0f),
                                           new Vector3(-9.5f, 0.0f, 35.0f),
                                           new Vector3(-18.5f, -6.0f, 35.0f),
                                           new Vector3(-16.5f, -3.0f, 35.0f),
                                           new Vector3(-12.5f, -12.0f, 35.0f),
                                       };

                private Laser[] waypoints = new Laser[8];

        //setup skybox
        private Model skyBox;
        private Matrix[] sbTransform;
        //private Vector3 sbPos = Vector3.Zero;
        private Vector3 sbPos = new Vector3(-15.0f, 0.0f, 15.0f);


        private Random random = new Random();

        private KeyboardState lastState;
        private int bossHealth;

        // Set the position of the camera in world space, for our view matrix.
        Camera freeCamera;
        Camera mainCamera;
        bool myCam = false;

        //player attributes
        int pHealth = 50;
        int count = 0;
        int totalCount = 0;


        private void InitializeTransform()
        {
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            freeCamera = new Camera(this, new Vector3(10.0f, 1.0f, 1.0f), Vector3.Zero, 5.0f);
            Components.Add(freeCamera);
            mainCamera = new Camera(this, new Vector3(10.0f, 0.0f, -10.0f), Vector3.Zero, 5.0f);
            Components.Add(mainCamera);
        }

        private void gameControls()
        {
            KeyboardState keyboardState = Keyboard.GetState();

            // Create some velocity if the right trigger is down.
            Vector3 mdlVelocityAdd = Vector3.Zero;

            // Find out what direction we should be thrusting, using rotation.
            mdlVelocityAdd.X = -(float)Math.Sin(0);
            mdlVelocityAdd.Y = -(float)Math.Tan(1);



            if (keyboardState.IsKeyDown(Keys.Down) || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y <= -0.5f)
            // if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                mdlVelocityAdd *= 0.01f;
                mdlVelocity += mdlVelocityAdd;
                GamePad.SetVibration(PlayerIndex.One, 0.1f, 0.0f);

                if (mdlPosition.Y <= GameConstants.PlayfieldminY)
                {
                    mdlVelocityAdd *= 0.0f;
                    mdlVelocity *= mdlVelocityAdd;
                }
            }

            if (keyboardState.IsKeyDown(Keys.Up) || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y >= 0.5f)
            {
                mdlVelocityAdd *= -0.01f;
                mdlVelocity += mdlVelocityAdd;
                GamePad.SetVibration(PlayerIndex.One, 0.1f, 0.0f);

                if (mdlPosition.Y >= GameConstants.PlayfieldmaxY)
                {
                    mdlVelocityAdd *= 0.0f;
                    mdlVelocity *= mdlVelocityAdd;
                }
            }

            if (GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y == 0.0f)
            {
                GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
            }



            if (keyboardState.IsKeyDown(Keys.R) || GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed)
            {
                mdlVelocity = Vector3.Zero;
                mdlPosition = new Vector3 (35.0f, 1.0f, 35.0f);
                mdlRotation = 0.0f;
                ResetDFG();
                myCam = false;
                bossHealth = 50;
                count = 0;
                totalCount = 0;
                pHealth = 50;
                laserList[0].isActive = false;
                //tardisSoundInstance.Play();
            }


            //are we shooting?
            if (keyboardState.IsKeyDown(Keys.Space) && lastState.IsKeyUp(Keys.Space) || GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed)
            {
                DateTime startVib;
                TimeSpan endVib;
                //add another bullet.  Find an inactive bullet slot and use it
                //if all bullets slots are used, ignore the user input
                for (int i = 0; i < GameConstants.NumLasers; i++)
                {
                    if (laserList[i].isActive)
                    {
                        //GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.5f);
                        //startVib = DateTime.Now;
                        //endVib = DateTime.Now - startVib;
                        //if (endVib.TotalSeconds >= 0.5f)
                        //{
                        //    GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.5f);
                        //}
                    }
                    if (!laserList[i].isActive)
                    {
                        Matrix tardisTransform = Matrix.CreateRotationX(90);
                        laserList[i].direction = tardisTransform.Left;
                        laserList[i].speed = GameConstants.LaserSpeedAdjustment;
                        laserList[i].position = mdlPosition + laserList[i].direction;
                        laserList[i].isActive = true;
                        boltFire.Play(0.1f, 0.0f, 0.0f);
                        break; //exit the loop     
                    }
                }


            }

            if (keyboardState.IsKeyDown(Keys.D1) && lastState.IsKeyUp(Keys.D1) || GamePad.GetState(PlayerIndex.One).Buttons.Y == ButtonState.Pressed)
            {
                //TOFIX - Stop the infinite change
                if (myCam)
                {
                    myCam = false;
                    mainCamera.Position = new Vector3(10.0f, 0.0f, -10.0f);
                }
                else
                {
                    myCam = true;
                    freeCamera.Position = new Vector3(10.0f, 1.0f, 1.0f);
                }

            }

            if (myCam == false)
            {
                if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.D))
                {
                    mainCamera.Position = new Vector3(10.0f, 1.0f, 1.0f);
                }
            }


            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            lastState = keyboardState;

        }

        private void ResetDFG()
        {
            Matrix dfgRot = new Matrix();
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
                    effect.LightingEnabled = true; // turn on the lighting subsystem.
                    effect.AmbientLightColor = new Vector3(10.0f, 10.0f, 10.0f);

                    if (myCam)
                    {
                        effect.Projection = freeCamera.Projection;
                        effect.View = freeCamera.View;

                    }
                    else
                    {
                        effect.Projection = mainCamera.Projection;
                        effect.View = mainCamera.View;
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
                        effect.Projection = freeCamera.Projection;
                        effect.View = freeCamera.View;
                    }
                    else
                    {
                        effect.Projection = mainCamera.Projection;
                        effect.View = mainCamera.View;
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

        

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            this.IsMouseVisible = false;
            Window.Title = "Blue Meanies v The Yellow Submarine";
            bossHealth = 50;
            InitializeTransform();
            ResetDFG();
            base.Initialize();
        }

        

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



            //-------------------------------------------------------------
            // added to load Model
            //-------------------------------------------------------------
            mdlTardis = Content.Load<Model>(".\\Models\\YellowSubmarine");
            mdlTardisTransforms = SetupEffectTransformDefaults(mdlTardis);
            mdlDFG = Content.Load<Model>(".\\Models\\DFG");
            mdDFGTramsforms = SetupEffectTransformDefaults(mdlDFG);
            mdlLaser = Content.Load<Model>(".\\Models\\laser");
            mdlLaserTransforms = SetupEffectTransformDefaults(mdlLaser);
            mdlBoss = Content.Load<Model>(".\\Models\\toaster");
            mdlBossTransforms = SetupEffectTransformDefaults(mdlBoss);

            skyBox = Content.Load<Model>(".\\SkyBox\\skybox2");
            sbTransform = SetupEffectTransformDefaults(skyBox);

            
            //-------------------------------------------------------------
            // added to load SoundFX's
            //-------------------------------------------------------------
            boltFire = Content.Load<SoundEffect>(".\\Audio\\bolt");
            bossHit = Content.Load<SoundEffect>(".\\Audio\\bossHit");

            //-------------------------------------------------------------
            // added to load energy bars
            //-------------------------------------------------------------

            healthBar = Content.Load<Texture2D>(".\\Others\\HealthBar");
            bossBar = Content.Load<Texture2D>(".\\Others\\BossBar");

            // TODO: use this.Content to load your game content here
        }


        bool soundToggle = true;

        private void playSongs()
        {
                switch (CurrentState)
                {

                    case GameState.Play:
                        gameTheme = Content.Load<Song>(".\\Audio\\YSIP");
                        MediaPlayer.Play(gameTheme);
                        MediaPlayer.IsRepeating = true;
                        break;

                    case GameState.Boss:
                        gameTheme = Content.Load<Song>(".\\Audio\\MOTM");
                        MediaPlayer.Play(gameTheme);
                        MediaPlayer.IsRepeating = true;
                        break;

                    case GameState.Win:
                        gameTheme = Content.Load<Song>(".\\Audio\\64");
                        MediaPlayer.Play(gameTheme);
                        MediaPlayer.IsRepeating = true;
                        break;

                }
        }



        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        private void toggleSong()
        {
            if (soundToggle)
            {
                MediaPlayer.IsMuted = true;
            }
            else
            {
                MediaPlayer.IsMuted = false;
            }
        }


        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            float posSpeed = GameConstants.posSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            float negSpeed = GameConstants.negSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (keyboardState.IsKeyDown(Keys.T) && lastState.IsKeyUp(Keys.T))
            {
                soundToggle = !soundToggle;
                toggleSong();
            }


            switch (CurrentState)
            {
                case GameState.Play:


                    if (MediaPlayer.State == MediaState.Stopped)
                    {
                        playSongs();
                    }

                    gameControls();
                    mdlPosition += mdlVelocity;
                    mdlVelocity *= 0.95f;

                    if (count == 10)
                    {
                        ResetDFG();
                        count = 0;
                    }

                    updateModels(timeDelta);

                    collisionCheck();

                    if (pHealth == 0)
                        this.Exit();

                    if (totalCount == dfgList.Length)
                    {
                        gameBoss = true;
                        MediaPlayer.Stop();
                        CurrentState = GameState.Boss;
                    }


                    base.Update(gameTime);
                    break;

                case GameState.Boss:

                    if (MediaPlayer.State == MediaState.Stopped)
                    {
                        playSongs();
                    }


                    gameControls();
                    mdlPosition += mdlVelocity;
                    mdlVelocity *= 0.95f;

                    if (count == 10)
                    {
                        ResetDFG();
                        count = 0;
                    }

                    updateModels(timeDelta);
                    moveBoss(timeDelta);

                    collisionCheck();

                    if (pHealth == 0)
                        this.Exit();

                    if (bossHealth == 0)
                    {
                        MediaPlayer.Stop();
                        CurrentState = GameState.Win;
                    }



                    base.Update(gameTime);
                    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                        this.Exit();
                    break;


                case GameState.Win:
                    if (MediaPlayer.State == MediaState.Stopped)
                    {
                        playSongs();
                    }
                    break;
            }
        }


        private void updateModels(float timeDelta)
        {
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
        }



        private void collisionCheck()
        {
            BoundingSphere TardisSphere =
             new BoundingSphere(mdlPosition,
                      mdlTardis.Meshes[0].BoundingSphere.Radius *
                            GameConstants.ShipBoundingSphereScale);

            BoundingSphere bossSphere = new BoundingSphere(bossPos, 
                mdlBoss.Meshes[0].BoundingSphere.Radius * 0.05f); //0.05

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

                            if (bossSphere.Intersects(laserSphere) && gameBoss)
                            {
                                laserList[k].isActive = false;
                                bossHealth = bossHealth - 10;
                                bossHit.Play(0.2f, 0.0f, 0.0f);
                            }


                        }

                        if (dalekSphereA.Intersects(TardisSphere)) //Check collision between Dalek and Tardis
                        {
                            //explosionSound.Play();
                            dfgList[i].direction *= -1.0f;
                            dfgList[i].isActive = false;
                            pHealth = pHealth - 2;
                            count++;
                            totalCount = totalCount + 1;
                            //laserList[k].isActive = false;
                            break; //no need to check other bullets
                        }
                    }
                }
            }
        }



        static int n = 0;
        private void moveBoss(float delta)
        {
            float speed = 5.0f * delta;

            BoundingSphere bossSphere = new BoundingSphere(bossPos, mdlBoss.Meshes[0].BoundingSphere.Radius * 0.08f);//0.8

           // if (gameBoss)
            //{
                waypoints[n].position = bossPoints[n];
                waypoints[n].isActive = true;

                BoundingSphere wpSphere = new BoundingSphere(waypoints[n].position, mdlLaser.Meshes[0].BoundingSphere.Radius * 0.001f);

                if (bossSphere.Intersects(wpSphere))// && n < bossPoints.Length)
                {
                    n = n + 1;
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
                    bossPos.X += -speed;
                }
                if (bossPos.Y < bossPoints[n].Y)
                {
                    bossPos.Y += speed;
                }
                if (bossPos.Y > bossPoints[n].Y)
                {
                    bossPos.Y += -speed;
             //   }
            }
        }






        Texture2D healthBar;
        Texture2D bossBar;

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
          spriteBatch.Begin();
            Matrix sbmodelTransform = Matrix.CreateRotationY(38) * Matrix.CreateTranslation(sbPos) * Matrix.CreateScale(30.0f);
            DrawModel(skyBox, sbmodelTransform, sbTransform);

            Matrix modelTransform = Matrix.CreateRotationY(mdlRotation) * Matrix.CreateTranslation(mdlPosition);
            DrawModel(mdlTardis, modelTransform, mdlTardisTransforms);

            //spriteBatch.Draw(healthBar, new Rectangle((int)mdlPosition.X + 1, (int)mdlPosition.Y + 200, (int)(200 * ((double)pHealth / 100)), 5), Color.Yellow);
            spriteBatch.Draw(healthBar, new Rectangle(20, 470, (int)(200 * ((double)pHealth / 100)), 5), Color.Yellow);


            switch (CurrentState)
            {
                case GameState.Play:
                    for (int i = 0; i < GameConstants.NumDaleks; i++)
                    {
                        if (dfgList[i].isActive)
                        {
                            Matrix dalekTransform = Matrix.CreateRotationY(20) * Matrix.CreateScale(GameConstants.DalekScalar) * Matrix.CreateTranslation(dfgList[i].position);
                            DrawModel(mdlDFG, dalekTransform, mdDFGTramsforms);
                        }
                    }


                    for (int i = 0; i < GameConstants.NumLasers; i++)
                    {
                        if (laserList[i].isActive)
                        {
                            Matrix laserTransform = Matrix.CreateScale(GameConstants.LaserScalar) * Matrix.CreateTranslation(laserList[i].position);
                            DrawModel(mdlLaser, laserTransform, mdlLaserTransforms);
                        }
                    }
                break;


                case GameState.Boss:


                    for (int i = 0; i < GameConstants.NumDaleks; i++)
                    {
                        if (dfgList[i].isActive)
                        {
                            Matrix dalekTransform = Matrix.CreateRotationY(20) * Matrix.CreateScale(GameConstants.DalekScalar) * Matrix.CreateTranslation(dfgList[i].position);
                            DrawModel(mdlDFG, dalekTransform, mdDFGTramsforms);
                        }
                    }


                    for (int i = 0; i < GameConstants.NumLasers; i++)
                    {
                        if (laserList[i].isActive)
                        {
                            Matrix laserTransform = Matrix.CreateScale(GameConstants.LaserScalar) * Matrix.CreateTranslation(laserList[i].position);
                            DrawModel(mdlLaser, laserTransform, mdlLaserTransforms);
                        }
                    }

                    Matrix bossTransform = Matrix.CreateScale(GameConstants.BossScalar) * Matrix.CreateRotationY(180) * Matrix.CreateTranslation(bossPos);
                        DrawModel(mdlBoss, bossTransform, mdlBossTransforms);


                        //spriteBatch.Draw(healthBar, new Rectangle((int)mdlPosition.X + 1, (int)mdlPosition.Y + 200, (int)(200 * ((double)pHealth / 100)), 5), Color.Yellow);
                        spriteBatch.Draw(bossBar, new Rectangle(680, 470, (int)(200 * ((double)bossHealth / 100)), 5), Color.Blue);

                break;

            }



  
            spriteBatch.End();

            writeText("Yellow Sub Vs Blue Meanies", new Vector2(50, 10), Color.Yellow);
            //writeText(pHealth.ToString(), new Vector2(50, 50), Color.Yellow);
            writeText(soundToggle.ToString(), new Vector2(50, 50), Color.Yellow);
            writeText(totalCount.ToString(), new Vector2(50, 75), Color.Yellow);
            writeText(bossHealth.ToString(), new Vector2(50, 100), Color.Yellow);


            base.Draw(gameTime);
        }
    }
}



//TODO:
/*
FIX VIBRATION
*/