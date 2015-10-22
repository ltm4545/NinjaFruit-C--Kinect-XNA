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
using Microsoft.Kinect;
using Kinect.Toolbox;
using Kinect.Toolbox.Record;
using FruitNinja;

namespace FruitNinja
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class KinectComponent : Microsoft.Xna.Framework.GameComponent
    {
        // variables
        //DateTime now;
        //DateTime past;
        KinectSensor kinect;
        Skeleton[] skeletonData;
        public int KinectID = -1;
        public bool cuted = false;
        public Microsoft.Xna.Framework.Vector3 head = new Microsoft.Xna.Framework.Vector3();
        public Microsoft.Xna.Framework.Vector3 rightHand = new Microsoft.Xna.Framework.Vector3();

        // generate background
        GraphicsDeviceManager graphics;

        public GraphicsDeviceManager Graphics
        {
            set { this.graphics = value; }
        }

        protected Texture2D colorVideo;

        readonly BarycenterHelper barycenterHelper = new BarycenterHelper();
        readonly AlgorithmicPostureDetector algoPostureRecognizer = new AlgorithmicPostureDetector();
        SwipeGestureDetector swipeGestureDetector;

        // methods change kinect coordinates system into 3D screen coordinates system
        public static Microsoft.Xna.Framework.Vector3 KinectOffset = new Microsoft.Xna.Framework.Vector3(0, 0, 0);
        public static bool LeftRotateBool = false;
        public static bool RightRotateBool = false;
        public static bool UpsideBool = false;

        public Texture2D getVideo() 
        {
            return this.colorVideo;
        }

        // rotate to left
        public void LeftRotate(Microsoft.Xna.Framework.Vector3 kinenctVector)
        {
            Microsoft.Xna.Framework.Vector3 headTmp;
            headTmp = head;
            head.X = headTmp.Z + kinenctVector.X;
            head.Y = headTmp.Y + kinenctVector.Y;
            head.Z = -headTmp.X + kinenctVector.Z;
        }

        // rotate to right
        public void RightRotate(Microsoft.Xna.Framework.Vector3 kinenctVector)
        {
            Microsoft.Xna.Framework.Vector3 headTmp;
            headTmp = head;
            head.X = -headTmp.Z + kinenctVector.X;
            head.Y = headTmp.Y + kinenctVector.Y;
            head.Z = headTmp.X + kinenctVector.Z;
        }

        // look upside
        public void Upside(Microsoft.Xna.Framework.Vector3 kinectVector)
        {
            Microsoft.Xna.Framework.Vector3 headTmp;
            headTmp = head;
            head.X = headTmp.X + kinectVector.X;
            head.Y = -headTmp.Y + kinectVector.Y;
            head.Z = headTmp.Z + kinectVector.Z;
        }

        // translation
        public void Translation(Microsoft.Xna.Framework.Vector3 kinectVector)
        {
            head.X += kinectVector.X;
            head.Y += kinectVector.Y;
            head.Z += kinectVector.Z;
        }

        public Microsoft.Xna.Framework.Vector3 Headposition
        {
            get { return new Microsoft.Xna.Framework.Vector3((float)head.X, (float)head.Y, (float)head.Z); }
        }

        public Microsoft.Xna.Framework.Vector3 Handposition
        {
            get { return new Microsoft.Xna.Framework.Vector3((float)rightHand.X, (float)rightHand.Y, (float)rightHand.Z); }
        }

        public float HeadDepth
        {
            get { return (float)head.Z; }
        }

        public bool Cuted
        {
            get { return cuted; }
            set { this.cuted = value; }
        }

        public KinectComponent(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            Console.WriteLine("Kinect component will be create\n");
            if (KinectSensor.KinectSensors.Count > 0) 
            {
                kinect = (from sensor in KinectSensor.KinectSensors
                          where sensor.Status == KinectStatus.Connected
                          select sensor).FirstOrDefault();
                // enable skeleton 
                kinect.SkeletonStream.Enable();
                // enable color background
                kinect.ColorStream.Enable(ColorImageFormat.RgbResolution1280x960Fps12);
                kinect.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(kinect_ColorFrameReady);

                kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_SkeletonFrameReady);
                Console.WriteLine("kinenct has been actived\n");
                kinect.Start();
            }
            // recognizer for hand
            swipeGestureDetector = new SwipeGestureDetector();
            // swipe event listener  
            swipeGestureDetector.OnGestureDetected += OnGestureDetected;
            base.Initialize();
        }

        void OnGestureDetected(string gesture)
        {
            // define differents gesture and respond it
            switch (gesture)
            {
                // define the action here
                case "SwipeToRight":
                    Console.WriteLine("Catch swipe gesture to RIGHT\n");
                    cuted = true;
                    break;
                case "SwipeToLeft":
                    Console.WriteLine("Catch swipe gesture to LEFT\n ");
                    cuted = true;
                    break;
            }
        }

        void processFrame(ReplaySkeletonFrame frame) 
        {
            //bool flag = false;
            foreach (var user in frame.Skeletons)
            {
                if (user.TrackingState == SkeletonTrackingState.NotTracked)
                    continue;
                else 
                {
                    barycenterHelper.Add(user.Position.ToVector3(), user.TrackingId);
                    if (!barycenterHelper.IsStable(user.TrackingId))
                        continue;
                    else 
                    {
                        foreach (Joint joint in user.Joints)
                        {
                            if (joint.TrackingState != JointTrackingState.Tracked)
                                continue;
                            // catch head position
                            if (joint.JointType == JointType.Head) 
                            {
                                // change coordinates 
                                KinectOffset.X = joint.Position.X;
                                KinectOffset.Y = joint.Position.Y;
                                KinectOffset.Z = joint.Position.Z;

                                if (KinectOffset.X > head.X)
                                    LeftRotate(KinectOffset);
                                else if (KinectOffset.X < head.X)
                                    RightRotate(KinectOffset);
                                else if (KinectOffset.Y > head.Y)
                                    Upside(KinectOffset);
                                else
                                    Translation(KinectOffset);

                                head.X = joint.Position.X;
                                head.Y = joint.Position.Y;
                                head.Z = -joint.Position.Z;                            
                                //Console.WriteLine("Head position X: " + head.X + " Head position Y: " + head.Y + " Head position Z: " + head.Z + "\n");
                            }
                            // catch right hand position
                            if (joint.JointType == JointType.HandRight)
                            {
                                rightHand.X = joint.Position.X;
                                rightHand.Y = joint.Position.Y;
                                rightHand.Z = -joint.Position.Z;
                            }
                            // catch right hand position
                            if (joint.JointType == JointType.HandRight)
                            {
                                swipeGestureDetector.Add(joint.Position, kinect);
                            }
                        }
                        algoPostureRecognizer.TrackPostures(user);
                    }
                }
            }
        }

        void kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            if (!((KinectSensor)sender).SkeletonStream.IsEnabled)
                return;

            SkeletonFrame skeletonFrame = e.OpenSkeletonFrame();
            if (skeletonFrame == null)
                return;

            Tools.GetSkeletons(skeletonFrame, ref skeletonData);
            if (skeletonData.All(s => s.TrackingState == SkeletonTrackingState.NotTracked))
                return;

            processFrame(skeletonFrame);
        }

        void kinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            ColorImageFrame videoFrame = e.OpenColorImageFrame();
            if (videoFrame != null)
            {
                // create array for pixel data and copy it from image frame
                Byte[] pixelData = new Byte[videoFrame.PixelDataLength];
                videoFrame.CopyPixelDataTo(pixelData);
                //Convert RGBA to BGRA
                Byte[] bgraPixelData = new Byte[videoFrame.PixelDataLength];
                for (int i = 0; i < pixelData.Length; i += 4)
                {
                    bgraPixelData[i] = pixelData[i + 2];
                    bgraPixelData[i + 1] = pixelData[i + 1];
                    bgraPixelData[i + 2] = pixelData[i];
                    bgraPixelData[i + 3] = (Byte)255;
                    //The video comes with 0 alpha so it is transparent
                }
                //Console.WriteLine("width : " + videoFrame.Width + " height: " + videoFrame.Height + "\n");
                colorVideo = new Texture2D(graphics.GraphicsDevice, videoFrame.Width, videoFrame.Height);
                //Console.WriteLine("texture suceess!\n");
                colorVideo.SetData(bgraPixelData);
            }
            else {
                Console.WriteLine("Frame null!\n");
            }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            base.Update(gameTime);
        }
    }
}
