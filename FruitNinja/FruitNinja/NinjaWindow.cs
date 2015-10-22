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

namespace FruitNinja
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class NinjaWindow : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // variable
        private Model model;
        private Model knife;
        // modify world matrix to change the object position
        private Matrix world = Matrix.CreateTranslation(new Vector3(0, 0, 0));
        private Matrix fruitWorld = Matrix.CreateTranslation(new Vector3(10, 10, -15));

        private Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 9), new Vector3(0, 0, 0), Vector3.UnitY);
        private Matrix fruitView = Matrix.CreateLookAt(new Vector3(0, 0, 9), new Vector3(0, 0, 0), Vector3.UnitY);

        private Matrix proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 1280f / 900f, 0.1f, 100f);
        private Matrix fruitProj = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 1280f / 900f, 0.1f, 100f);
        //Matrix view1, proj1, illuMatix;

        private Random random;
       
        // kinect handle
        KinectComponent kinect;

        // variable used to update 3D environment
        public static bool kinectbool = true;
        private bool cuted = false;
        Vector3 fruitPos = new Vector3(10, 10, -15);
        Vector3 hand = new Vector3(0, 0, 0);
        Vector3 gameHand;
        //Vector3 target = new Vector3(0, 0, 0);
        //Vector3 up = Vector3.UnitY;

        public NinjaWindow()
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
            // initialize kinenct component
            kinect = new KinectComponent(this);
            kinect.Graphics = this.graphics;
            // THE GAME COMPONENT MUST BE ADDED 
            this.Components.Add(kinect);
            // WHEN WE ADD THE COMPONENT, IT WAS ALREADY INITIALIZED
            //kinect.Initialize();
            random = new Random();
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
            // TODO: use this.Content to load your game content here
            model = Content.Load<Model>("pear");
            knife = Content.Load<Model>("knife");
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

            // TODO: Add your update logic here
            graphics.PreferredBackBufferHeight = 900;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.ApplyChanges();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {   
            if (kinectbool && kinect != null)
            { 
                // get hand position of kinenct component
                hand = kinect.Handposition;
                // transforme the kinect position into game world position
                gameHand = new Vector3(hand.X * 20 * (1280 / 640), hand.Y * 40 * (900 / 480), hand.Z * 30);
                this.world = Matrix.CreateTranslation(gameHand);
                // check if a fruit is been cuted
                if (CheckCuted() && kinect.Cuted)
                {
                    NewFruit();
                    kinect.Cuted = false;
                }
            }

            // TODO: Add your drawing code here
            GraphicsDevice.Clear(Color.CornflowerBlue);
            // draw kinect background
           
            Texture2D video = kinect.getVideo();
            if (video != null)
            { 
                spriteBatch.Begin();
                spriteBatch.Draw(kinect.getVideo(), new Rectangle(0, 0, 1280, 900), Color.White);
                spriteBatch.End();
            }
            else 
            {
                Console.WriteLine("Can't catch video stream\n");
            }
            DrawModel(model, fruitWorld, fruitView, fruitProj);
            DrawModel(knife, world, view, proj);
            base.Draw(gameTime);
        }

        private void DrawModel(Model model, Matrix world, Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects) 
                {
                    effect.World = world;
                    effect.View = view;
                    effect.Projection = projection;
                }              
                mesh.Draw();
            }
        }

        public bool CheckCuted() 
        {
            // debug
            Console.WriteLine("Hand position X: " + hand.X * 20 + " hand position Y: " + hand.Y * 40 + " Hand position Z: " + hand.Z *5 + "\n");
            Console.WriteLine("fruit position X: " + fruitPos.X + " fruit position Y: " + fruitPos.Y + " fruit position Z: " + fruitPos.Z + "\n");
            // compare hand position interval and fruite position
            if (Math.Abs(hand.X * 20 - fruitPos.X) < 5 && Math.Abs(hand.Y * 40 - fruitPos.Y) < 10 && Math.Abs(hand.Z *5 - fruitPos.Z) < 10) 
            {
                Console.WriteLine("Cuted\n");
                return true;
            }
            return false;
        }

        public void NewFruit() 
        {
            // debug
            Console.WriteLine("We are going to modify the position of fruit\n");
            // change position of new fruit
            fruitPos = new Vector3(random.Next(-8, 8), random.Next(-8, 8), random.Next(-10, 0));
            while (Math.Abs(hand.X * 20 - fruitPos.X) < 5 && Math.Abs(hand.Y * 40 - fruitPos.Y) < 10 && Math.Abs(hand.Z * 5 - fruitPos.Z) < 10)
            {
                fruitPos = new Vector3(random.Next(-8, 8), random.Next(-8, 8), random.Next(-10, 0));
            }
            fruitWorld = Matrix.CreateTranslation(fruitPos);
        }
    }
}
