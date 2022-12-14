using System;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Threading;

namespace ADPCM {
    internal class WaveOut : WaveLib, IDisposable {
        FileStream mFs;
        VAG mAdpcmL = new VAG();
        VAG mAdpcmR = new VAG();
        short[] mBuffL;
        short[] mBuffR;
        int mPackingSize;
        bool mStop = false;
        bool mStopped = true;
        int mLoadSize = 0;

        public int Channels = 2;
        public long FileSize { get { return mFs.Length; } }
        public long Position {
            get { return mFs.Position; }
            set { mFs.Position = value; }
        }
        public int PackingSize { get { return mPackingSize; } }

        public WaveOut(string filePath, int sampleRate, int packingSize = 0x800) : base(
            sampleRate,
            2,
            (packingSize < 128 ? 128 : packingSize) * VAG.PACKING_SAMPLES * 2 >> 4
        ) {
            mPackingSize = packingSize;
            mBuffL = new short[VAG.PACKING_SAMPLES * mPackingSize >> 4];
            mBuffR = new short[VAG.PACKING_SAMPLES * mPackingSize >> 4];
            mFs = new FileStream(filePath, FileMode.Open);
        }

        public new void Dispose() {
            Stop();
            if (null != mFs) {
                mFs.Close();
                mFs.Dispose();
                mFs = null;
            }
            base.Dispose();
        }

        public void Stop() {
            mStop = true;
            while (!mStopped) {
                Thread.Sleep(10);
            }
        }

        public void Start() {
            mStop = false;
            mStopped = false;
        }

        public void SetProperty(long position, int channels, int packingSize) {
            Stop();

            mFs.Position = position;
            Channels = channels;
            mPackingSize = packingSize;
            mBuffL = new short[VAG.PACKING_SAMPLES * mPackingSize >> 4];
            mBuffR = new short[VAG.PACKING_SAMPLES * mPackingSize >> 4];

            Start();
        }

        protected override void SetData() {
            if (mStopped) {
                Array.Clear(WaveBuffer, 0, WaveBuffer.Length);
                return;
            }
            int pos = 0;
            while (pos < BufferSize) {
                if (mLoadSize < mBuffL.Length) {
                    load();
                }
                for (int j = 0; j < mBuffL.Length && pos < BufferSize; j++) {
                    WaveBuffer[pos] = mBuffL[j];
                    WaveBuffer[pos + 1] = mBuffR[j];
                    pos += 2;
                    mLoadSize--;
                }
            }
            if (mStop) {
                mStopped = true;
            }
        }

        void load() {
            for (int i = 0, j = 0; i < mPackingSize; i += 16, j += VAG.PACKING_SAMPLES) {
                mFs.Read(mAdpcmL.EncBuf, 0, mAdpcmL.EncBuf.Length);
                mAdpcmL.Dec(mAdpcmL.EncBuf);
                Array.Copy(mAdpcmL.DecBuf, 0, mBuffL, j, VAG.PACKING_SAMPLES);
            }
            if (1 == Channels) {
                Array.Copy(mBuffL, 0, mBuffR, 0, mBuffR.Length);
            }
            if (2 <= Channels) {
                for (int i = 0, j = 0; i < mPackingSize; i += 16, j += VAG.PACKING_SAMPLES) {
                    mFs.Read(mAdpcmR.EncBuf, 0, mAdpcmR.EncBuf.Length);
                    mAdpcmR.Dec(mAdpcmR.EncBuf);
                    Array.Copy(mAdpcmR.DecBuf, 0, mBuffR, j, VAG.PACKING_SAMPLES);
                }
            }
            for (int c = 2; c < Channels; c++) {
                mFs.Position += mPackingSize;
            }
            if (FileSize <= Position) {
                mStopped = true;
            }
            mLoadSize = VAG.PACKING_SAMPLES * mPackingSize >> 4;
        }
    }
}
