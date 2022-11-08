using System;
using System.Windows.Forms;
using System.IO;

namespace ADPCM {
    public partial class Form1 : Form {
        string mFilePath = "";
        WaveOut mWave;

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            timer1.Interval = 33;
            timer1.Enabled = true;
            timer1.Start();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            if (null != mWave) {
                mWave.Dispose();
            }
        }

        private void btnOpen_Click(object sender, EventArgs e) {
            openFileDialog1.ShowDialog();
            var filePath = openFileDialog1.FileName;
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) {
                return;
            }
            mFilePath = filePath;
            Text = mFilePath;
            if (null != mWave) {
                mWave.Dispose();
            }
            mWave = new WaveOut(mFilePath, (int)numSampleRate.Value, (int)numPackingSize.Value * 16);
            mWave.Channels = (int)numChannels.Value;
            var len = mWave.FileSize / mWave.PackingSize / 2;
            var div = 100;
            if (0 == len) {
                len = 1;
            }
            if (len < div) {
                div = 1;
            }
            trackbar1.Value = 0;
            trackbar1.MaxValue = len;
            trackbar1.MinorTickFreq = len / div;
            trackbar1.MajorTickFreq = trackbar1.MinorTickFreq * 10;
            mWave.Start();
            btnPlay.Text = "一時停止";
        }

        private void btnPlay_Click(object sender, EventArgs e) {
            if (null == mWave) {
                btnPlay.Text = "再生";
                return;
            }

            if ("再生" == btnPlay.Text) {
                mWave.Start();
                btnPlay.Text = "一時停止";
            } else {
                mWave.Stop();
                btnPlay.Text = "再生";
            }
        }

        private void timer1_Tick(object sender, EventArgs e) {
            if (null == mWave) {
                return;
            }
            if(trackbar1.IsDrag) {
                mWave.Position = trackbar1.Value * mWave.PackingSize * 2;
            } else {
                trackbar1.Value = mWave.Position / mWave.PackingSize / 2;
            }
        }

        private void numChannels_ValueChanged(object sender, EventArgs e) {
            if (null == mWave) {
                return;
            }
            mWave.Channels = (int)numChannels.Value;
        }
    }
}
