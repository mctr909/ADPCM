using System;
using System.Windows.Forms;
using System.IO;

namespace ADPCM {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            var fw = new FileStream("C:\\Users\\ris\\Desktop\\test.bin", FileMode.Create);
            var fi = new FileStream("C:\\Users\\ris\\Desktop\\test.csv", FileMode.Create);
            var sw = new StreamWriter(fi);
            var enc = new VAG();
            var dec = new VAG();
            var wave = new short[28];
            double count = 0.0;
            for (int j=0; j<100; j++) {
                for (int i = 0; i < VAG.PACKING_SAMPLES; i++) {
                    //wave[i] = (short)(32000 * Math.Sin(2 * Math.PI * count));
                    if (count < 0.5) {
                        wave[i] = 32000;
                    } else {
                        wave[i] = -32000;
                    }
                    count += 560 / 44100.0;
                    if (1 <= count) {
                        count -= 1.0;
                    }
                }
                enc.Enc(wave);
                dec.Dec(enc.EncBuf);
                fw.Write(enc.EncBuf, 0, enc.EncBuf.Length);
                for (int i = 0; i < VAG.PACKING_SAMPLES; i++) {
                    sw.WriteLine(dec.DecBuf[i]);
                }
            }
            fw.Close();
            fw.Dispose();
            fi.Close();
            fi.Dispose();
        }
    }
}
