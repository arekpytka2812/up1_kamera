using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Windows.Forms;
using AForge.Video;
using Accord.Video.FFMPEG;
using System.IO;
using AForge.Vision.Motion;
using AForge.Imaging.Filters;
using AForge.Math.Random;

namespace UP1
{
    public partial class Form1 : Form
    {
        FilterInfoCollection videoDeviceList;
        VideoCaptureDevice capturedDevice;
        VideoFileWriter videoFileWriter;
        Bitmap frame;
        Double zoom = 0.5;

        HueModifier hueFilter = new HueModifier(180);
        SaturationCorrection satFilter = new SaturationCorrection(0.5f);

        bool isCameraOn = false;
        bool isRecording = false;

        int photoCount = 1;
        int filmCount = 1;

        string defaultPhotoPath = @"C:/Users/arpy2/Desktop/zdjecie";
        string defaultFilmPath = @"C:/Users/arpy2/Desktop/video";

        public Form1()
        {
            InitializeComponent();

            // creating devices list and adding them to combobox
            videoDeviceList = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            foreach (FilterInfo videoDevice in videoDeviceList)
            {
                comboBoxDevices.Items.Add(videoDevice.Name);
            }

            // creating new capture device
            capturedDevice = new VideoCaptureDevice();

            // prevents user from not choosing any device
            comboBoxDevices.SelectedIndex = 0;

            // changing flags and buttons' statuses
            buttonStop.Enabled = false;
            buttonStartRecording.Enabled = false;
            buttonStopRecording.Enabled = false;
            buttonSave.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // getting chosen device and starting signal
            capturedDevice = new VideoCaptureDevice(videoDeviceList[comboBoxDevices.SelectedIndex].MonikerString);
            capturedDevice.NewFrame += new NewFrameEventHandler(video_NewFrame);
            capturedDevice.Start();

            // changing flags and buttons' statuses
            isCameraOn = true;
            buttonStart.Enabled = false;
            buttonStop.Enabled = true;
            buttonStartRecording.Enabled = true;
            buttonSave.Enabled = true;
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // cloning frame and applying various filters
            frame = (Bitmap)eventArgs.Frame.Clone();
            frame = new Bitmap(frame, new Size((int)(frame.Width * zoom), (int)(frame.Height * zoom)));
            frame = hueFilter.Apply(frame);
            frame = satFilter.Apply(frame);
            pictureBox1.Image = frame;

            // saving frame for recorded film
            if (isRecording)
                videoFileWriter.WriteVideoFrame((Bitmap)frame.Clone());  
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            // stoping signal
            capturedDevice.SignalToStop();
            
            // "clearing" picturebox
            pictureBox1.Image = null;

            // changing flags and buttons' statuses
            isCameraOn = false;
            buttonStart.Enabled = true;
            buttonStop.Enabled = false;
            buttonSave.Enabled = false;
            buttonStartRecording.Enabled = false;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            // creating path for saved frame
            var path = defaultPhotoPath + photoCount.ToString() + ".bmp";
            photoCount++;

            // deleting if path exists
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            // cloning saved frame
            Bitmap imageToSave = (Bitmap)pictureBox1.Image.Clone();

            // saving frame
            imageToSave.Save(path);
        }

        private void buttonStartRecording_Click(object sender, EventArgs e)
        {
            // creating path for recorded film
            var path = defaultFilmPath + filmCount.ToString() + ".avi";
            filmCount++;

            // deleting if path exists
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            // creating VideoFileWriter instance
            videoFileWriter = new VideoFileWriter();

            // opening stream
            videoFileWriter.Open(path, pictureBox1.Image.Width, pictureBox1.Image.Height, 13, VideoCodec.MPEG4);

            // changing flags and buttons' statuses
            isRecording = true;
            buttonStartRecording.Enabled = false;
            buttonStopRecording.Enabled = true;

            // changing recordingLabel text
            labelRecordingStatus.Text = "Recording!";
        }

        private void buttonStopRecording_Click(object sender, EventArgs e)
        {
            // closing VideoFileWriter stream
            videoFileWriter.Close();

            // changing flags and buttons' statuses
            buttonStartRecording.Enabled = true;
            buttonStopRecording.Enabled = false;
            isRecording = false;

            // setting recordingStatusLabel to null
            labelRecordingStatus.Text = "";
        }


        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            // setting up saturation filter value
            satFilter.AdjustValue = (float)trackBar2.Value / 10;
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            // setting up hue filter value
            hueFilter.Hue = trackBar3.Value;
        }

        
    }
}
