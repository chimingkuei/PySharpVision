﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using Basler.Pylon;
using OpenCvSharp;

namespace PySharpVision
{
    class Basler
    {
        public Camera camera = null;
        private PixelDataConverter converter = new PixelDataConverter();
        private Stopwatch stopWatch = new Stopwatch();
        public PictureBox Display = new PictureBox();
        public string image_storage_path;
        public Bitmap result;

        private void OnImageGrabbed(Object sender, ImageGrabbedEventArgs e)
        {
            try
            {
                IGrabResult grabResult = e.GrabResult;
                if (grabResult.IsValid)
                {
                    // Reduce the number of displayed images to a reasonable amount if the camera is acquiring images very fast.
                    if (!stopWatch.IsRunning || stopWatch.ElapsedMilliseconds > 33)
                    {
                        stopWatch.Restart();
                        Bitmap bitmap = new Bitmap(grabResult.Width, grabResult.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                        // Lock the bits of the bitmap.
                        System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bitmap.PixelFormat);
                        // Place the pointer to the buffer of the bitmap.
                        converter.OutputPixelFormat = PixelType.BGRA8packed;
                        IntPtr ptrBmp = bmpData.Scan0;
                        converter.Convert(ptrBmp, bmpData.Stride * bitmap.Height, grabResult);
                        bitmap.UnlockBits(bmpData);
                        // Assign a temporary variable to dispose the bitmap after assigning the new bitmap to the display control.
                        Bitmap bitmapOld = Display.Image as Bitmap;
                        // Provide the display control with the new bitmap. This action automatically updates the display.
                        //Display.Image = bitmap;
                        result = bitmap;
                        //Thread.Sleep(500);
                        if (bitmapOld != null)
                        {
                            // Dispose the bitmap.
                            bitmapOld.Dispose();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ShowException(exception);
            }
            finally
            {
                // Dispose the grab result if needed for returning it to the grab loop.
                e.DisposeGrabResultIfClone();
            }
        }

        private void ShowException(Exception exception)
        {
            System.Windows.MessageBox.Show("Exception caught:\n" + exception.Message, "Error");
        }

        public void Init(out bool state)
        {
            try
            {
                camera = new Camera();
                camera.StreamGrabber.ImageGrabbed += OnImageGrabbed;
                camera.CameraOpened += Configuration.AcquireContinuous;
                state = true;
            }
            catch (Exception exception)
            {
                ShowException(exception);
                state = false;
            }
        }

        public void OpenCamera()
        {
            Console.WriteLine("Using camera {0}.", camera.CameraInfo[CameraInfoKey.ModelName]);
            camera.Open();
            Console.WriteLine("Camera Width {0}.", camera.Parameters[PLCamera.Width]);
            Console.WriteLine("Camera Height {0}.", camera.Parameters[PLCamera.Height]);
        }

        public void ContinueAcquisition()
        {
            try
            {
                // Start the grabbing of images until grabbing is stopped.
                Configuration.AcquireContinuous(camera, null);
                camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            }
            catch (Exception exception)
            {
                ShowException(exception);
            }
        }

        public void StopAcquisition()
        {
            // Stop the grabbing.
            try
            {
                camera.StreamGrabber.Stop();
                Display.Image = null;
            }
            catch (Exception exception)
            {
                ShowException(exception);
            }
        }

    }






}
