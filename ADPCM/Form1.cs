using System;
using System.Windows.Forms;
using System.IO;

namespace ADPCM {
    public partial class Form1 : Form {
        string mFilePath = "";
        FileStream mFs;

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {

        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            if (null != mFs) {
                mFs.Close();
                mFs.Dispose();
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
            if (null != mFs) {
                mFs.Close();
                mFs.Dispose();
            }
            mFs = new FileStream(mFilePath, FileMode.Open, FileAccess.Read);
            var len = mFs.Length / 16;
            trackbar1.Value = 0;
            trackbar1.MaxValue = len;
            trackbar1.MinorTickFreq = len / 80;
            trackbar1.MajorTickFreq = trackbar1.MinorTickFreq * 10;
        }

        private void btnPlay_Click(object sender, EventArgs e) {
            if (null == mFs) {
                btnPlay.Text = "再生";
                return;
            }

            if ("再生" == btnPlay.Text) {
                btnPlay.Text = "停止";
            } else {
                btnPlay.Text = "再生";
            }
        }
    }
}
