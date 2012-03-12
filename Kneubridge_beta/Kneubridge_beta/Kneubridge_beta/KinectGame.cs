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

namespace Kneubridge_beta
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class KinectGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        KinectSensor _kinectSensor;
        Texture2D _kinectRGBVideo;
        String _connectedStatus = "Not connected";
        Texture2D _overlay;
        SpriteFont _font;

        
        public KinectGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 640;
            graphics.PreferredBackBufferHeight = 480;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //This event handler allows you to run code once the status changes on any of the connected devices. 
            //This listens to the statuschanged event on the kinect sensor making it possible to check the statuses
            //of all connected kinects.
            KinectSensor.KinectSensors.StatusChanged += new EventHandler<StatusChangedEventArgs>(KinectSensors_StatusChanged);
            DiscoverKinectSensor();
            
            base.Initialize();
        }

        void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            if (this._kinectSensor == e.Sensor)
            {
                if (e.Status == KinectStatus.Disconnected ||
                    e.Status == KinectStatus.NotPowered)
                {
                    this._kinectSensor = null;
                    this.DiscoverKinectSensor();
                }
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Load game content
            _kinectRGBVideo = new Texture2D(GraphicsDevice, 1337, 1337);
            _overlay = this.Content.Load<Texture2D>("overlay");
            _font = this.Content.Load<SpriteFont>("SpriteFont1");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            _kinectSensor.Stop();
            _kinectSensor.Dispose();
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

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Kinect drawing code
            spriteBatch.Begin();
            spriteBatch.Draw(_kinectRGBVideo, new Rectangle(0, 0, 640, 480), Color.White);
            spriteBatch.Draw(_overlay, new Rectangle(0, 0, 640, 480), Color.White);
            spriteBatch.DrawString(_font, _connectedStatus, new Vector2(20, 80), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Iterates through all connected Kinect devices and when a device is found set the global kinectSensor
        /// instance, update the status message and initialize it once connected.
        /// </summary>
        private void DiscoverKinectSensor()
        {
            foreach (KinectSensor sensor in KinectSensor.KinectSensors)
            {
                if (sensor.Status == KinectStatus.Connected)
                {
                    // Found one, set our sensor to this
                    _kinectSensor = sensor;
                    break;
                }
            }

            if (this._kinectSensor == null)
            {
                _connectedStatus = "Found none Kinect Sensors connected to USB";
                return;
            }

            // You can use the kinectSensor.Status to check for status
            // and give the user some kind of feedback
            switch (_kinectSensor.Status)
            {
                case KinectStatus.Connected:
                    {
                        _connectedStatus = "Status: Connected";
                        break;
                    }
                case KinectStatus.Disconnected:
                    {
                        _connectedStatus = "Status: Disconnected";
                        break;
                    }
                case KinectStatus.NotPowered:
                    {
                        _connectedStatus = "Status: Connect the power";
                        break;
                    }
                default:
                    {
                        _connectedStatus = "Status: Error";
                        break;
                    }
            }

            // Init the found and connected device
            if (_kinectSensor.Status == KinectStatus.Connected)
            {
                InitializeKinect();
            }
        }

        /// <summary>
        /// Enables RGB Camera in the Kinect Sensor for use.
        /// Initializes Kinect object to get the needed streams. Configure and enable ColorStream to notify us when a new image
        /// is ready from the Kinect RGB camera. Once notification is recieved we can start the device.
        /// </summary>
        /// <returns>Success status - boolean</returns>
        private bool InitializeKinect()
        {
            _kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            _kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(kinectSensor_ColorFrameReady);
              
            try
            {
                _kinectSensor.Start();
            }
            catch
            {
                _connectedStatus = "Unable to start the Kinect Sensor";
                return false;
            }
            return true;
        }

        /// <summary>
        /// Captures the image from the Kinect sensor, creates a Color array, fills it with the data from the captures image for 
        /// each pixel, and then stores it in a Texture2d object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void kinectSensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorImageFrame = e.OpenColorImageFrame())
            {
                if (colorImageFrame != null)
                {

                    byte[] pixelsFromFrame = new byte[colorImageFrame.PixelDataLength];

                    colorImageFrame.CopyPixelDataTo(pixelsFromFrame);

                    Color[] color = new Color[colorImageFrame.Height * colorImageFrame.Width];
                   _kinectRGBVideo = new Texture2D(graphics.GraphicsDevice, colorImageFrame.Width, colorImageFrame.Height);

                    // Go through each pixel and set the bytes correctly.
                    // Remember, each pixel got a Rad, Green and Blue channel.
                    int index = 0;
                    for (int y = 0; y < colorImageFrame.Height; y++)
                    {
                        for (int x = 0; x < colorImageFrame.Width; x++, index += 4)
                        {
                            color[y * colorImageFrame.Width + x] = new Color(pixelsFromFrame[index + 2], pixelsFromFrame[index + 1], pixelsFromFrame[index + 0]);
                        }
                    }

                    // Set pixeldata from the ColorImageFrame to a Texture2D
                    _kinectRGBVideo.SetData(color);
                }
            }
        }

    }
}
