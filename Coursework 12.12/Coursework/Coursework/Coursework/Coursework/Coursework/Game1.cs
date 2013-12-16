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
        // GUI Variables
        //------------------------------------------
        SpriteFont gameFont; //font used to draw text on screen
        private Texture2D healthBar; //image used to represent health bar
        private Texture2D bossBar; //image used to represent bosses health bar
        private Texture2D mainMenu;

        //--------------------------------------------------
        // Added for use with playing Audio via Media player
        //--------------------------------------------------
        private Song gameTheme; //variable used to declare song. This song is the games main theme

        //--------------------------------------------------
        //Set the sound effects to use
        //--------------------------------------------------
        private SoundEffect boltFire; //the sound effect used when the player fires a bolt
        private SoundEffect bossHit; //the sound effect used when a bolt hits the game boss

        //--------------------------------------------------
        // Gamestates usd to manage the game
        //--------------------------------------------------
        private enum GameState
        {
            Menu,
            Play, 
            Boss, 
            Win,
            Lose,
        }
        GameState CurrentState = GameState.Menu; //the gamestates for the game. Initialised the state to "play"

        //--------------------------------------------------
        //Player Variables
        //--------------------------------------------------
        private Model mdlYS; //creating the model for the player
        private Matrix[] ysTransforms; //creating the matrix to transform the player model 
        int pHealth = 50; //the amount of health the plaer has
        private Vector3 ysPosition = new Vector3(35.0f, 1.0f, 35.0f); //initialising the position og the player
        private float ysRotation = 0.1f; //initialising the rotation of the player
        private Vector3 ysVelocity = Vector3.Zero; //initialising player velocity

        //--------------------------------------------------
        // Enemy Variables (Dreadful Flying Glove)
        //--------------------------------------------------
        private Model mdlDFG; //create a model for the enemies
        private Matrix[] mdDFGTramsforms; //Setting up a matrix transformation for the enemies
        private Enemies[] dfgList = new Enemies[GameConstants.NumDFG]; //creates an array of Dreadful Flying Floves from the Enemy class

        //--------------------------------------------------
        // Boss Variables
        //--------------------------------------------------
        private Model mdlBoss; //creates a model for the enemy boss
        private Matrix[] mdlBossTransforms; //creates a matrix transformation for the boss
        private Vector3 bossPos = new Vector3 (-30.0f, -5.0f, 35.0f); //specifies the bosses position
        private bool gameBoss = false; //variable to determine if the Gameboss is active or not
        private int bossHealth; //initialises the health of the boss

        //--------------------------------------------------
        // Bolt Variables
        //--------------------------------------------------
        private Model mdlBolt; //creates a model for the bolts
        private Matrix[] boltTransforms; //sets up a matrix to transform the bolt
        private Bolt[] boltList = new Bolt[GameConstants.NumBolts]; //creates an array of bolts from the class Bolt

        //--------------------------------------------------
        // Waypoint Variables
        //--------------------------------------------------
        private Vector3[] bossPoints = {
                                    new Vector3(-15.0f, 3.0f, 35.0f), 
                                    new Vector3(-10.0f, 13.0f, 35.0f),
                                    new Vector3(-12.5f, 19.0f, 35.0f),
                                    new Vector3(-13.5f, 7.0f, 35.0f),
                                    new Vector3(-9.5f, 0.0f, 35.0f),
                                    new Vector3(-18.5f, -6.0f, 35.0f),
                                    new Vector3(-16.5f, -3.0f, 35.0f),
                                    new Vector3(-12.5f, -12.0f, 35.0f),
                                }; //series of waypoints for the boss to travel between
         
        private Bolt[] waypoints = new Bolt[8]; //creates a array of waypoints. Uses the Bolt class so that the waypoints can implement its methods

        //--------------------------------------------------
        // Skybox Variables
        //--------------------------------------------------
        //setup skybox
        private Model skyBox; //creates model for the skybox
        private Matrix[] sbTransform; //sets up matrix to transform the skybox
        private Vector3 sbPos = new Vector3(-15.0f, 0.0f, 15.0f); //defines position of the skybox

        //--------------------------------------------------
        // Camera Variables
        //--------------------------------------------------
        private Camera freeCamera; //creates secondary camera
        private Camera mainCamera; //creates the main camera. 
        private float aspectRatio; // The aspect ratio determines how to scale 3d to 2d projection.
        bool myCam = false; //creates a boolean to alter between cameras

        //--------------------------------------------------
        // Game Variables
        //--------------------------------------------------
        int count = 0; //number of DFG that have been defeated by the enemy
        int totalCount = 0; //total number of DFG defeat by the enemy
        private Random random = new Random();
        private KeyboardState lastState; //creates an instance of keyboard state
        private bool soundToggle = false; //boolean used to toggle the songs in the game.
        private bool fogOn = true;

#endregion

        #region User Defined Methods

        private void InitializeTransform()
        {
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio; //sets the aspect ratio
            freeCamera = new Camera(this, new Vector3(10.0f, 1.0f, 1.0f), Vector3.Zero, 5.0f); //initialsies the free-form camera
            Components.Add(freeCamera); //adds it to the game component
            mainCamera = new Camera(this, new Vector3(10.0f, 0.0f, -10.0f), Vector3.Zero, 0.0f); //initialises the main camera. This is initialised second so that it is the main camera
            Components.Add(mainCamera);
        }

        private void gameControls()
        {
            KeyboardState keyboardState = Keyboard.GetState(); //current state of the keyboard

            Vector3 ysVelocityAdd = Vector3.Zero; //creates velocity

            // Find out what direction we should be thrusting, using rotation.
            ysVelocityAdd.Y = -(float)Math.Tan(1);


            if (keyboardState.IsKeyDown(Keys.Down) || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y <= -0.5f) //move player down
            {
                ysVelocityAdd *= 0.01f;
                ysVelocity += ysVelocityAdd; //adds velocity and moves the player
                GamePad.SetVibration(PlayerIndex.One, 0.1f, 0.0f); //vibrates controller

                if (ysPosition.Y <= GameConstants.PlayfieldminY) //prevents the player from moving outside the world boundaries
                {
                    ysVelocityAdd *= 0.0f; //set velocity to 0
                    ysVelocity *= ysVelocityAdd;
                }
            }


            if (keyboardState.IsKeyDown(Keys.Up) || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y >= 0.5f) //move player up
            {
                ysVelocityAdd *= -0.01f; //creates velocity
                ysVelocity += ysVelocityAdd; //adds velocity and moves the player
                GamePad.SetVibration(PlayerIndex.One, 0.1f, 0.0f); //vibrates controller

                if (ysPosition.Y >= GameConstants.PlayfieldmaxY) //prevents the player from moving outside the world boundaries
                {
                    ysVelocityAdd *= 0.0f; //set velocity to 0
                    ysVelocity *= ysVelocityAdd;
                }
            }

            if (GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y == 0.0f)
            {
            }



            if (keyboardState.IsKeyDown(Keys.R) || GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed) //resets the game
            {
                gameReset();
            }

            if (keyboardState.IsKeyDown(Keys.Space) && lastState.IsKeyUp(Keys.Space) || GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed) //fires the bolt
            {
                DateTime startVib;
                TimeSpan endVib;

                //add another bullet.  Find an inactive bullet slot and use it
                //if all bullets slots are used, ignore the user input
                for (int i = 0; i < GameConstants.NumBolts; i++)
                {   
                    if (boltList[i].isActive)
                    {
                        //GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.5f);
                        //startVib = DateTime.Now;
                        //endVib = DateTime.Now - startVib;
                        //if (endVib.TotalSeconds >= 0.5f)
                        //{
                        //    GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.5f);
                        //}
                    }
                    if (!boltList[i].isActive) //creates the bolts if there are none active in the game
                    {
                        Matrix tardisTransform = Matrix.CreateRotationX(90);
                        boltList[i].direction = tardisTransform.Left; //sets direction of the bolt, travels from left to right
                        boltList[i].speed = GameConstants.BoltSpeedAdjustment; //sets the speed of the bolt
                        boltList[i].position = ysPosition + boltList[i].direction; //updates the position with the bolt direction
                        boltList[i].isActive = true; //activates the bolts
                        boltFire.Play(0.1f, 0.0f, 0.0f); //plays SFX
                        break; //exit the loop     
                    }
                }


            }

            if (keyboardState.IsKeyDown(Keys.D1) && lastState.IsKeyUp(Keys.D1) || GamePad.GetState(PlayerIndex.One).Buttons.Y == ButtonState.Pressed) //change the camera
            {
                if (myCam) //changes the state of myCam and resets camera positions
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

            if (myCam == false) //prevents the main camera from being moved
            {
                if (keyboardState.IsKeyDown(Keys.W) ||
                    keyboardState.IsKeyDown(Keys.A) ||
                    keyboardState.IsKeyDown(Keys.S) ||
                    keyboardState.IsKeyDown(Keys.D))
                {
                    mainCamera.Position = new Vector3(10.0f, 1.0f, -10.0f); //resets the camera position for each keypress
                }
            }


            if (keyboardState.IsKeyDown(Keys.Escape)) //exits the game
                this.Exit();

            if (keyboardState.IsKeyDown(Keys.F) && lastState.IsKeyUp(Keys.F) || GamePad.GetState(PlayerIndex.One).Buttons.B == ButtonState.Pressed)
                fogOn = !fogOn;


            lastState = keyboardState; //sets lastState to Keyboardstate. Prevents input from happening 60 times a second

        }

        private void ResetDFG()
        {
            Matrix dfgRot = new Matrix(); //creates rotation matrix for the DFG
            for (int i = 0; i < GameConstants.NumDFG; i++) //for loop to set up all DFG in the array
            {
                dfgList[i].position = new Vector3((-30.0f - (i*2)), (1.0f * (i*5)), 35.0f); //creates unique position for each DFG. Values increase on X and Y axis to space enemies out
                double angle = random.NextDouble() * 2 * Math.PI; //creates random angle
                dfgList[i].direction.X = 1; //sets up direction
                dfgList[i].speed = (GameConstants.DFGMinSpeed + 0.21f * GameConstants.DFGMaxSpeed) / 5; //sets the speed of the DFG
                dfgList[i].rotation = dfgRot.Left; //sets the rotation of the DFG
                dfgList[i].isActive = true; //activates the DFG arrY
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
                    effect.AmbientLightColor = new Vector3(1.0f, 1.0f, 1.0f);

                    if (fogOn) //allows the user to toggle fog on and off
                    {
                        effect.FogEnabled = true; //enables fog on the screen
                    }
                    else
                    {
                        effect.FogEnabled = false;
                    }

                    effect.FogColor = Color.DarkSlateBlue.ToVector3(); //colours the fog using an XNA colour and translates the RGB value to a Vector3
                    effect.FogStart = 1390.75f; //distance that the fog starts from the camera
                    effect.FogEnd = 1411.25f; //distance the fog ends from the camera


                    if (myCam) // applies the appropriate projection and view matrices depending on what camera is active
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
            Vector2 FontOrigin = gameFont.MeasureString(output) / 2;
            Vector2 FontPos = msgPos;
            // Draw the string
            spriteBatch.DrawString(gameFont, output, FontPos, msgColour);
            spriteBatch.End();
        }

        private void updateModels(float timeDelta)
        {
            for (int i = 0; i < GameConstants.NumDFG; i++) //loops through each DFG in the list and updates
            {
                dfgList[i].Update(timeDelta);
            }

            for (int i = 0; i < GameConstants.NumBolts; i++) //determines is a bolt is on screen.
            {
                if (boltList[i].isActive) //if so it updates
                {
                    boltList[i].Update(timeDelta);
                }
            }
        }

        private void collisionCheck() //checks for a serious of collisions
        {
            BoundingSphere ysSphere =
             new BoundingSphere(ysPosition,
                      mdlYS.Meshes[0].BoundingSphere.Radius *
                            GameConstants.YSBoundingSphereScale); //Bounding Sphere for the player

            BoundingSphere bossSphere = new BoundingSphere(bossPos, //Bounding Sphere for the Boss
                mdlBoss.Meshes[0].BoundingSphere.Radius * 0.05f); //0.05

            //THIS IS WHERE COLLISION CHECKING BEGINS
            for (int i = 0; i < dfgList.Length; i++)
            {
                if (dfgList[i].isActive)
                {
                    BoundingSphere dfgSphereA =
                      new BoundingSphere(dfgList[i].position, mdlDFG.Meshes[0].BoundingSphere.Radius *
                                     GameConstants.DFGBoundingSphereScale); //creates bounding spheres for the DFG

                    for (int k = 0; k < boltList.Length; k++) //loop for the bolts
                    {
                        if (boltList[k].isActive) //determines if a bolt is active
                        {
                            BoundingSphere boltSphere = new BoundingSphere(
                              boltList[k].position, mdlBolt.Meshes[0].BoundingSphere.Radius *
                                     GameConstants.BoltBoundingSphereScale); //if so it creates a sphere for the bolt

                            if (dfgSphereA.Intersects(boltSphere)) //check for collision between a bolt and the DFG
                            {
                                dfgList[i].isActive = false; //removes the DFG
                                boltList[k].isActive = false; //Removes the bolt
                                count++; //increases count
                                totalCount++; //increases total count
                                break; //no need to check other bullets
                            }


                            if (bossSphere.Intersects(boltSphere) && gameBoss)//check for collision between the boss and the bolt IF gameboss is true
                            {
                                boltList[k].isActive = false; //removes the bolt
                                bossHealth = bossHealth - 10; //reduces the bosses health
                                bossHit.Play(0.2f, 0.0f, 0.0f); //plays a sound effect
                            }


                        }

                        if (dfgSphereA.Intersects(ysSphere)) //Check collision between dfg and the player
                        {
                            dfgList[i].isActive = false; //removes DFG
                            pHealth = pHealth - 25;//reuces player health
                            count++; //increments count
                            totalCount++; //increases total count
                            break; //no need to check other DFG
                        }
                    }
                }
            }
        }

        static int n = 0; //variable declared outside method so value is not reset per frame
        private void moveBoss(float delta) //updates the boss position
        {
            float speed = 5.0f * delta; //sets speed

            BoundingSphere bossSphere = new BoundingSphere(bossPos, mdlBoss.Meshes[0].BoundingSphere.Radius * 0.08f);//creates a smaller bounding sphere for the boss for waypoint collision
            //0.8

            waypoints[n].position = bossPoints[n]; //sets the array of waypoint positions to equal the array of bossPoints
            waypoints[n].isActive = true; //activates the waypoints

            BoundingSphere wpSphere = new BoundingSphere(waypoints[n].position, mdlBolt.Meshes[0].BoundingSphere.Radius * 0.001f); //creates bounding sphere for waypoints

            if (bossSphere.Intersects(wpSphere))//checks for collision between the boss and the waypoint
            {
                n = n + 1; //increments n to loop through the waypoints
            }
            if (n == bossPoints.Length) //checks that n is less than the length of boss points
            {
                n = 0; //if it is equal to it n is reset
            }

            //changes the x position of the boss. Determined by the x position of he waypoint
            if (bossPos.X < bossPoints[n].X)
            {
                bossPos.X += speed;
            }
            if (bossPos.X > bossPoints[n].X)
            {
                bossPos.X += -speed;
            }
            //changes the x position of the boss. Determined by the x position of he waypoint
            if (bossPos.Y < bossPoints[n].Y)
            {
                bossPos.Y += speed;
            }
            if (bossPos.Y > bossPoints[n].Y)
            {
                bossPos.Y += -speed;
            }
        }

        private void playSongs()
        {
            switch (CurrentState) //switch statement. Determines how to act depending on the gamestate. This allows different songa to play during different states
            {

                case GameState.Play: //instructions for "Play"
                    gameTheme = Content.Load<Song>(".\\Audio\\YSIP"); //sets the gameTheme to YSIP
                    MediaPlayer.Play(gameTheme); //plays the gametheme
                    MediaPlayer.IsRepeating = true; //repeats the theme
                    break;

                case GameState.Boss:
                    gameTheme = Content.Load<Song>(".\\Audio\\MOTM"); //sets the gametheme to MOTM
                    MediaPlayer.Play(gameTheme); //plays the gametheme
                    MediaPlayer.IsRepeating = true;
                    break;

                case GameState.Win:
                    gameTheme = Content.Load<Song>(".\\Audio\\64"); //sets the theme to 64
                    MediaPlayer.Play(gameTheme); //plays the theme
                    MediaPlayer.IsRepeating = true;
                    break;

            }
        }

        private void toggleSong() //method o toggle the sound
        {
            if (soundToggle)
            {
                MediaPlayer.IsMuted = true; //mutes if soundToggle is true
            }
            else
            {
                MediaPlayer.IsMuted = false;//unmutes sound
            }
        }

        private void gameReset() //method used to reset the game
        {
            ysVelocity = Vector3.Zero; //nulls velocity
            ysPosition = new Vector3(35.0f, 1.0f, 35.0f); //resets player position
            ysRotation = 0.0f; //returns rotation to 0
            ResetDFG(); //calls this method so that enemies are created
            myCam = false; //returns camera to mainCamera
            bossHealth = 50; //reset bosshealth
            count = 0; //reset count
            totalCount = 0; //resets totalCount
            pHealth = 50; //resets health
            boltList[0].isActive = false; //deletes any bolts
            bossPos = new Vector3(-30.0f, -5.0f, 35.0f); //resets the bosses position
            CurrentState = GameState.Menu; //resets the current state to menu
        }

        #endregion






        public Game1() //Constructor
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            this.IsMouseVisible = false;
            Window.Title = "Blue Meanies v The Yellow Submarine"; //Text in Title Bar
            bossHealth = 50;
            InitializeTransform(); //Calls this method
            ResetDFG(); //calls this method to create enemies
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
            gameFont = Content.Load<SpriteFont>(".\\Fonts\\DrWho");

            //-------------------------------------------------------------
            // added to load Song
            //-------------------------------------------------------------
            gameTheme = Content.Load<Song>(".\\Audio\\YSIP");//gameTheme is assigned a default song. This changes when gamestates change

            //-------------------------------------------------------------
            // added to load Models
            //-------------------------------------------------------------
            mdlYS = Content.Load<Model>(".\\Models\\YellowSubmarine"); //player
            ysTransforms = SetupEffectTransformDefaults(mdlYS); //player transform
            mdlDFG = Content.Load<Model>(".\\Models\\DFG"); //Dreadful Flying Gloves
            mdDFGTramsforms = SetupEffectTransformDefaults(mdlDFG); //DFG Transform
            mdlBolt = Content.Load<Model>(".\\Models\\laser"); //Bolt
            boltTransforms = SetupEffectTransformDefaults(mdlBolt); //bolt transform
            mdlBoss = Content.Load<Model>(".\\Models\\toaster"); //Enemy Boss
            mdlBossTransforms = SetupEffectTransformDefaults(mdlBoss); //boss transform
            skyBox = Content.Load<Model>(".\\SkyBox\\skybox2"); //Sky Box
            sbTransform = SetupEffectTransformDefaults(skyBox); //sky box transform
            
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

            //-------------------------------------------------------------
            // added to load Menu Screen
            //-------------------------------------------------------------
            mainMenu = Content.Load<Texture2D>(".\\Others\\MenuBG");

        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState(); //creates a keyboar d state to be used for input
            float posSpeed = GameConstants.posSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds; //sets positive speed
            float negSpeed = GameConstants.negSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds; //sets negative speed

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds; //converts gameTime to float so we can pass this value

            if (keyboardState.IsKeyDown(Keys.T) && lastState.IsKeyUp(Keys.T) || GamePad.GetState(PlayerIndex.One).Buttons.X == ButtonState.Pressed) //command to toggle sound
            {
                soundToggle = !soundToggle;
                toggleSong();
            }


            switch (CurrentState) //switch statement determines what is updated in what state
            {

                case GameState.Menu:
                    if (keyboardState.IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                        this.Exit(); //option to exit the game

                    if (keyboardState.IsKeyDown(Keys.Space) || GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed) //controls to start the game
                        CurrentState = GameState.Play;
                    break;

                case GameState.Play: 
                    if (MediaPlayer.State == MediaState.Stopped) //checks to see if Mediaplayer has stopped. If it is then playSongs is called
                    {
                        playSongs();
                    }

                    gameControls(); //calls Gamecontrol method to check for input
                    ysPosition += ysVelocity; //updates player position
                    ysVelocity *= 0.95f; //bleeds off velocity

                    if (count == 10) //checks to see if count is 10
                    {
                        ResetDFG(); //if so more enemies are spawned
                        count = 0; //count is reset
                    }

                    updateModels(timeDelta); //calls updateModels

                    collisionCheck(); //checks for collisions

                    if (pHealth == 0) //game ends if the player health is 0
                        CurrentState = GameState.Lose;

                    if (totalCount == dfgList.Length) //checks to see if the total count is equal to 5 times the lengh of the dfgList
                    {
                        gameBoss = true; //changes value of gameboss
                        MediaPlayer.Stop(); //stops the media player
                        CurrentState = GameState.Boss; //changes gamestate
                    }


                    base.Update(gameTime);
                    break;

                case GameState.Boss:

                    if (MediaPlayer.State == MediaState.Stopped) //checks to see if Mediaplayer has stopped. If it is then playSongs is called
                    {
                        playSongs();
                    }


                    gameControls(); //calls game controls
                    ysPosition += ysVelocity; //changes player position
                    ysVelocity *= 0.95f; //bleeds velocity

                    if (count == 10) //checks to see if count is 10
                    {
                        ResetDFG(); //calls method to cspawn enemies
                        count = 0; //resets count
                    }

                    updateModels(timeDelta); //updates models
                    moveBoss(timeDelta); //updates the boss

                    collisionCheck(); //check for collisions

                    if (pHealth == 0) //exit game is the player does
                        CurrentState = GameState.Lose;

                    if (bossHealth == 0) //checks to see if boss health is 0
                    {
                        MediaPlayer.Stop(); //stops media player
                        CurrentState = GameState.Win; //changes gamestate
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

                    if (keyboardState.IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) //input to exit the game
                        this.Exit();

                    if (keyboardState.IsKeyDown(Keys.R) || GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed) //resets the game
                        gameReset();


                    //if the Gamestate is Win, do nothing.
                    break;
                
                case GameState.Lose:
                    if (keyboardState.IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) //input to exit the game
                        this.Exit();

                    if (keyboardState.IsKeyDown(Keys.R) || GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed) //resets the game
                        gameReset();

                    break;

            }
        }

        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            spriteBatch.Begin();

            Matrix sbmodelTransform = Matrix.CreateRotationY(38) * Matrix.CreateTranslation(sbPos) * Matrix.CreateScale(30.0f); //tramsforms skybox model
            DrawModel(skyBox, sbmodelTransform, sbTransform); //draws the model onscreen

            Matrix modelTransform = Matrix.CreateRotationY(ysRotation) * Matrix.CreateTranslation(ysPosition); //transforms player model
            DrawModel(mdlYS, modelTransform, ysTransforms); //draws model

            //spriteBatch.Draw(healthBar, new Rectangle((int)mdlPosition.X + 1, (int)mdlPosition.Y + 200, (int)(200 * ((double)pHealth / 100)), 5), Color.Yellow); //draws the player healthbar under the player
            spriteBatch.Draw(healthBar, new Rectangle(20, 470, (int)(200 * ((double)pHealth / 100)), 7), Color.Yellow); //draws the player healthbar at the bottom of the window
            spriteBatch.End();

            switch (CurrentState)
            {

                case GameState.Menu:
                    spriteBatch.Begin();
                    spriteBatch.Draw(mainMenu, Vector2.Zero, Color.White);//draws the menu background image
                    spriteBatch.End();
                    break;
                case GameState.Play: //the following things are drawn during GameState.Play



                    for (int i = 0; i < GameConstants.NumDFG; i++)//begin loop for DFG
                    {
                        if (dfgList[i].isActive) //checks to see if DFG is activr
                        {
                            Matrix dfgTransform = Matrix.CreateRotationY(20) * Matrix.CreateScale(GameConstants.DFGScalar) * Matrix.CreateTranslation(dfgList[i].position); //creates transform for DFG
                            DrawModel(mdlDFG, dfgTransform, mdDFGTramsforms); //draws DFG
                        }
                    }


                    for (int i = 0; i < GameConstants.NumBolts; i++) //begin loop for bolts
                    {
                        if (boltList[i].isActive) //checks to see if bolt list is active
                        {
                            Matrix bTransform = Matrix.CreateScale(GameConstants.BoltScalar) * Matrix.CreateTranslation(boltList[i].position); //creates transform for bolts
                            DrawModel(mdlBolt, bTransform, boltTransforms); //draws bolts
                        }
                    }
                break;


                case GameState.Boss:
                    for (int i = 0; i < GameConstants.NumDFG; i++) //begins the loop for DFG
                    {
                        if (dfgList[i].isActive) //checks to see if any are active
                        {
                            Matrix dfgTransform = Matrix.CreateRotationY(20) * Matrix.CreateScale(GameConstants.DFGScalar) * Matrix.CreateTranslation(dfgList[i].position); //transforms each in the loop
                            DrawModel(mdlDFG, dfgTransform, mdDFGTramsforms); //draws each in the loop
                        }
                    }


                    for (int i = 0; i < GameConstants.NumBolts; i++) //begins the loop for bolts
                    {
                        if (boltList[i].isActive) //checks to see if they are active
                        {
                            Matrix boltTrans = Matrix.CreateScale(GameConstants.BoltScalar) * Matrix.CreateTranslation(boltList[i].position); //transforms each bolt
                            DrawModel(mdlBolt, boltTrans, boltTransforms); //draws each bolt in the loop.
                        }
                    }

                        Matrix bossTransform = Matrix.CreateScale(GameConstants.BossScalar) * Matrix.CreateRotationY(180) * Matrix.CreateTranslation(bossPos); //transforms the boss
                        DrawModel(mdlBoss, bossTransform, mdlBossTransforms); //draws the boss model.


                        //spriteBatch.Draw(healthBar, new Rectangle((int)mdlPosition.X + 1, (int)mdlPosition.Y + 200, (int)(200 * ((double)pHealth / 100)), 5), Color.Yellow);
                        spriteBatch.Begin();
                        spriteBatch.Draw(bossBar, new Rectangle(680, 470, (int)(200 * ((double)bossHealth / 100)), 7), Color.LightBlue);
                        spriteBatch.End();

                break;



                case GameState.Lose: //utilises writeText to inform the player they have lost the game. 
                writeText("THE BLUE MEANIES HAVE DEFEATED SGT. PEPPER'S LONELY \nHEARTS CLUB BAND! PEPPERLAND IS NO LONGER SAFE!\nGAME OVER!", new Vector2(50, 100), Color.Yellow);
                writeText("Press [R/Start] to restart or [Esc/Back] to Exit.", new Vector2(50, 200), Color.Yellow);
                break;

                case GameState.Win: //utilises writeText to inform the player they have won the game
                writeText("You have defeated the Blue Meanies and Pepperland \nis safe once more. Well Done!", new Vector2(10, 100), Color.Yellow);
                writeText("Press [R/Start] to restart or [Esc/Back] to Exit.", new Vector2(10, 150), Color.Yellow);
                break;

            }
            base.Draw(gameTime);
        }
    }
}

//Vibration