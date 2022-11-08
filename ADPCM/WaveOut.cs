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
        long mPosition = 0;
        int mPackingSize = 16;
        int mChannels = 2;
        bool mStop = false;
        bool mStopped = true;
        int mLoadSize = 0;

        public long FileSize { get { return mFs.Length; } }
        public long Position { get { return mPosition; } }
        public int PackingSize { get { return mPackingSize; } }
        public new int Channels { get { return mChannels; } }

        public WaveOut(string filePath) : base() {
            mBuffL = new short[VAG.PACKING_SAMPLES * mPackingSize >> 4];
            mBuffR = new short[VAG.PACKING_SAMPLES * mPackingSize >> 4];
            mFs = new FileStream(filePath, FileMode.Open);
            mStopped = false;
        }

        public new void Dispose() {
            mStop = true;
            while (!mStopped) {
                Thread.Sleep(10);
            }
            if (null != mFs) {
                mFs.Close();
                mFs.Dispose();
            }
        }

        public void SetProperty(long position, int channels, int packingSize) {
            mStop = true;
            while(!mStopped) {
                Thread.Sleep(10);
            }

            mPosition = position;
            mChannels = channels;
            mPackingSize = packingSize;
            mBuffL = new short[VAG.PACKING_SAMPLES * mPackingSize >> 4];
            mBuffR = new short[VAG.PACKING_SAMPLES * mPackingSize >> 4];

            mStop = false;
            mStopped = false;
        }

        protected override void SetData() {
            if (mStopped) {
                return;
            }
            for (int i = 0; i < BufferSize; i += 2 * mBuffL.Length) {
                if (mLoadSize < mBuffL.Length) {
                    load();
                }
                for (int j = 0; j < mBuffL.Length && (i + j + 1) < BufferSize; j++) {
                    WaveBuffer[i + j] = mBuffL[j];
                    WaveBuffer[i + j + 1] = mBuffR[j];
                }
                mLoadSize -= mBuffL.Length;
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
            if (1 == mChannels) {
                Array.Copy(mBuffL, 0, mBuffR, 0, mBuffR.Length);
            }
            if (2 <= mChannels) {
                for (int i = 0, j = 0; i < mPackingSize; i += 16, j += VAG.PACKING_SAMPLES) {
                    mFs.Read(mAdpcmR.EncBuf, 0, mAdpcmR.EncBuf.Length);
                    mAdpcmR.Dec(mAdpcmR.EncBuf);
                    Array.Copy(mAdpcmR.DecBuf, 0, mBuffR, j, VAG.PACKING_SAMPLES);
                }
            }
            for (int c = 2; c < mChannels; c++) {
                mFs.Position += mPackingSize;
            }

            mLoadSize = VAG.PACKING_SAMPLES * mPackingSize >> 4;
            mPosition = mFs.Position;
        }
    }
}
