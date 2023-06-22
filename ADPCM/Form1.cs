using System;
using System.Windows.Forms;
using System.IO;

namespace ADPCM {
    public partial class Form1 : Form {
        WaveOut mWave;

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            numPlayChannel.Minimum = 0;
            numPlayChannel.Value = 0;
            numPlayChannel.Maximum = 0;
            numPlayChannel.Enabled = false;
            listBox1.SelectionMode = SelectionMode.One;
            listBox1.AllowDrop = true;
            timer1.Interval = 33;
            timer1.Enabled = true;
            timer1.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            if (null != mWave) {
                mWave.Dispose();
            }
        }

        private void btnPlay_Click(object sender, EventArgs e) {
            if (null == mWave) {
                if (0 == listBox1.Items.Count) {
                    return;
                }
                if (listBox1.SelectedIndex < 0) {
                    listBox1.SelectedIndex = 0;
                }
                var path = (string)listBox1.SelectedItem;
                if (!File.Exists(path)) {
                    return;
                }
                Text = path;
                load();
                trackbar1.Value = 0;
            }
            if ("再生" == btnPlay.Text) {
                play();
            } else {
                stop();
            }
        }

        private void btnEncode_Click(object sender, EventArgs e) {
            var dir = Path.GetDirectoryName(Text);
            var fileName = Path.GetFileNameWithoutExtension(Text);
            if (!ADPCM2.EncodeFile(Text, dir + "\\" + fileName + ".bin")) {
                ADPCM2.DecodeFile(Text, dir + "\\" + fileName + "_decode.wav");
            }
        }

        private void timer1_Tick(object sender, EventArgs e) {
            if (null == mWave) {
                lblPos.Text = string.Format("{0}/{1}packs\noffset:0x{2}", trackbar1.Value, trackbar1.MaxValue, 0);
                return;
            }
            if(trackbar1.IsDrag) {
                setPos();
            } else {
                trackbar1.Value = mWave.Position / mWave.PackingSize;
            }
            if (mWave.FileSize <= mWave.Position) {
                var itemIndex = listBox1.SelectedIndex;
                if (itemIndex < 0) {
                    mWave.Position = 0;
                    btnPlay.Text = "再生";
                    return;
                }
                if (listBox1.Items.Count <= itemIndex + 1) {
                    listBox1.SelectedIndex = 0;
                    return;
                }
                listBox1.SelectedIndex++;
            }
            lblPos.Text = string.Format("{0}/{1}packs\noffset:0x{2}", trackbar1.Value, trackbar1.MaxValue, mWave.Position.ToString("X"));
        }

        private void numChannels_ValueChanged(object sender, EventArgs e) {
            numPlayChannel.Minimum = 0;
            numPlayChannel.Value = 0;
            if ((int)numChannels.Value < 3) {
                numPlayChannel.Enabled = false;
                numPlayChannel.Maximum = 0;
            } else {
                numPlayChannel.Enabled = true;
                numPlayChannel.Maximum = numChannels.Value - 2;
            }
            if (null == mWave) {
                return;
            }
            mWave.Channels = (int)numChannels.Value;
            setPos();
        }

        private void numPlayChannel_ValueChanged(object sender, EventArgs e) {
            if (null == mWave) {
                return;
            }
            setPos();
        }

        private void btnApply_Click(object sender, EventArgs e) {
            if (string.IsNullOrEmpty(Text) || !File.Exists(Text)) {
                return;
            }
            load();
            if ("一時停止" == btnPlay.Text) {
                setPos();
                play();
            }
        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e) {
            if (MouseButtons.Right == e.Button) {
                var itemIndex = listBox1.IndexFromPoint(e.X, e.Y);
                if (itemIndex < 0) {
                    return;
                }
                var itemText = (string)listBox1.Items[itemIndex];
                var dde = listBox1.DoDragDrop(itemText, DragDropEffects.All);
                if (DragDropEffects.Move == dde) {
                    listBox1.Items.RemoveAt(itemIndex);
                }
            }
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e) {
            e.Effect = DragDropEffects.Move;
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) {
                return;
            }
            foreach (var filePath in (string[])e.Data.GetData(DataFormats.FileDrop)) {
                listBox1.Items.Add(filePath);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            var itemIndex = listBox1.SelectedIndex;
            if (itemIndex < 0) {
                return;
            }
            var filePath = (string)listBox1.Items[itemIndex];
            if (!File.Exists(filePath)) {
                return;
            }
            Text = filePath;
            load();
            trackbar1.Value = 0;
            play();
        }

        void setPos() {
            var div = mWave.PackingSize << 4;
            var pos = trackbar1.Value * mWave.PackingSize;
            var ofs = (int)numPlayChannel.Value * mWave.PackingSize;
            mWave.Position = pos / div * div + ofs;
        }

        void play() {
            btnPlay.Text = "一時停止";
            btnEncode.Enabled = false;
            mWave.Start();
        }

        void stop() {
            mWave.Stop();
            btnPlay.Text = "再生";
            btnEncode.Enabled = true;
        }

        void load() {
            if (null != mWave) {
                mWave.Dispose();
            }
            mWave = new WaveOut(Text, (int)numSampleRate.Value, (int)numPackingSize.Value << 4);
            mWave.Channels = (int)numChannels.Value;
            var len = mWave.FileSize / mWave.PackingSize;
            var div = 100;
            if (0 == len) {
                len = 1;
            }
            if (len < div) {
                div = 1;
            }
            trackbar1.MaxValue = len;
            trackbar1.MinorTickFreq = len / div;
            trackbar1.MajorTickFreq = trackbar1.MinorTickFreq * 10;
        }
    }
}
