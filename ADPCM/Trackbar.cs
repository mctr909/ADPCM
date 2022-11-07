using System;
using System.Drawing;
using System.Windows.Forms;

namespace ADPCM {
    public partial class Trackbar : UserControl {
        static readonly Brush BKNOB = new Pen(Color.FromArgb(127, 255, 0, 0)).Brush;
        static readonly Pen PKNOB = new Pen(Color.FromArgb(95, 31, 31, 31));
        static readonly Brush BSLIDER = new Pen(Color.FromArgb(95, 95, 95)).Brush;
        static readonly Pen PSLIDER = new Pen(Color.FromArgb(0, 0, 0));

        static readonly int KNOB_WIDTH = 5;
        static readonly int KNOB_HEIGHT = 12;
        static readonly Point[] KNOB_POLY = new Point[] {
            new Point(-KNOB_WIDTH, -(KNOB_HEIGHT - 5)),
            new Point(-KNOB_WIDTH, KNOB_HEIGHT - 5),
            new Point(0, KNOB_HEIGHT),
            new Point(KNOB_WIDTH, KNOB_HEIGHT - 5),
            new Point(KNOB_WIDTH, -(KNOB_HEIGHT - 5)),
            new Point(0, -KNOB_HEIGHT)
        };

        Point[] mKnobPoly = new Point[6];
        Graphics mG;
        bool mSizeChange = false;
        bool mDrag = false;
        Point mPos = new Point();

        public bool IsDrag { get { return mDrag; } }
        public int MajorTickFreq = 100;
        public int MinorTickFreq = 25;
        public long MaxValue = 1000;
        public long MinValue = 0;
        public long Value = 0;

        public Trackbar() {
            InitializeComponent();
            BackColor = Color.Transparent;
            pictureBox1.Left = 0;
            pictureBox1.Top = 0;
            setSize();
        }

        private void Trackbar_Load(object sender, EventArgs e) {
            timer1.Interval = 1;
            timer1.Enabled = true;
            timer1.Start();
        }

        private void Trackbar_SizeChanged(object sender, EventArgs e) {
            mSizeChange = true;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                mDrag = true;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e) {
            mDrag = false;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e) {
            mPos = e.Location;
        }

        void setSize() {
            pictureBox1.Width = Width;
            pictureBox1.Height = Height;
            if (null != pictureBox1.Image) {
                pictureBox1.Image.Dispose();
            }
            if (null != mG) {
                mG.Dispose();
            }
            pictureBox1.Image = new Bitmap(Width, Height);
            mG = Graphics.FromImage(pictureBox1.Image);
        }

        private void timer1_Tick(object sender, EventArgs e) {
            if (mSizeChange) {
                setSize();
                mSizeChange = false;
            }

            mG.Clear(Color.Transparent);

            var trkWidth = Width - KNOB_WIDTH * 2 - 1;
            var tickWidth = (double)trkWidth / (MaxValue - MinValue);

            long posX;
            var value = (int)((mPos.X - KNOB_WIDTH) / tickWidth + 0.5);
            if (mDrag) {
                posX = KNOB_WIDTH + value * trkWidth / (MaxValue - MinValue);
                if (value < MinValue) {
                    Value = MinValue;
                } else if (MaxValue < value) {
                    Value = MaxValue;
                } else {
                    Value = value;
                }
            } else {
                posX = KNOB_WIDTH + Value * trkWidth / (MaxValue - MinValue);
            }
            if (posX < KNOB_WIDTH) {
                posX = KNOB_WIDTH;
            }
            if (trkWidth + KNOB_WIDTH < posX) {
                posX = trkWidth + KNOB_WIDTH;
            }

            for (long i = (MaxValue - MinValue) / MinorTickFreq; i >= 0; i--) {
                var x = KNOB_WIDTH + (int)(i * tickWidth * MinorTickFreq);
                var h = (i % (MajorTickFreq / MinorTickFreq) == 0) ? 4 : 0;
                mG.DrawLine(Pens.Black, x, 4 - h, x, KNOB_HEIGHT * 2 + h);
            }

            mG.FillRectangle(BSLIDER, KNOB_WIDTH, KNOB_HEIGHT - 1, trkWidth, 6);
            mG.DrawRectangle(PSLIDER, KNOB_WIDTH, KNOB_HEIGHT - 1, trkWidth, 6);

            mG.DrawPolygon(PSLIDER, mKnobPoly);
            for (int i=0; i< KNOB_POLY.Length; i++) {
                mKnobPoly[i].X = KNOB_POLY[i].X + (int)posX;
                mKnobPoly[i].Y = KNOB_POLY[i].Y + KNOB_HEIGHT + 2;
            }
            mG.FillPolygon(BKNOB, mKnobPoly);
            mG.DrawPolygon(PKNOB, mKnobPoly);

            pictureBox1.Image = pictureBox1.Image;
        }
    }
}
