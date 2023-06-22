using System;
using System.IO;
using System.Threading;

namespace ADPCM {
    internal class WaveOut : WaveLib, IDisposable {
        string mFilePath = "";
        FileStream mFs;
        VAG mAdpcmL = new VAG();
        VAG mAdpcmR = new VAG();
        RiffWave mWave;
        short[] mBuffL;
        short[] mBuffR;
        int mPackingSize;
        bool mStop = false;
        bool mStopped = true;
        int mLoadSamples = 0;
        int mBufferSamples;
        long mPosition = 0;

        delegate void Loader();
        Loader mLoader;

        public int Channels = 2;
        public long FileSize {
            get {
                if (null != mWave && mWave.IsLoadComplete) {
                    return mWave.Length;
                } else if (null != mFs) {
                    return mFs.Length;
                } else {
                    return 1;
                }
            }
        }
        public long Position {
            get {
                if (null != mWave && mWave.IsLoadComplete) {
                    return mWave.Position;
                } else if (null != mFs) {
                    return mFs.Position;
                } else {
                    return 0;
                }
            }
            set {
                if (null != mWave && mWave.IsLoadComplete) {
                    mWave.Position = value;
                } else if (null != mFs) {
                    mFs.Position = value;
                }
                mPosition = value;
            }
        }
        public int PackingSize { get { return mPackingSize; } }

        public WaveOut(string filePath, int sampleRate, int packingSize = 0x800) {
            mFilePath = filePath;
            mPackingSize = packingSize;
            closeFile();
            openFile();
            mBuffL = new short[mBufferSamples];
            mBuffR = new short[mBufferSamples];
            if (mWave.IsLoadComplete) {
                mPosition = mWave.Position;
                Setup(mWave.SampleRate, 2, mBufferSamples);
            } else {
                mPosition = 0;
                Setup(sampleRate, 2, mBufferSamples);
            }
        }

        public new void Dispose() {
            Stop();
            base.Dispose();
        }

        public void Stop() {
            mStop = true;
            for (int i = 0; i < 50 && !mStopped; i++) {
                Thread.Sleep(10);
            }
            closeFile();
        }

        public void Start() {
            if (!mStop) {
                return;
            }
            openFile();
            if (mWave.IsLoadComplete) {
                mWave.Position = mPosition;
            } else {
                mFs.Position = mPosition;
            }
            mStop = false;
            mStopped = false;
        }

        protected override void SetData() {
            if (mStopped) {
                Array.Clear(WaveBuffer, 0, WaveBuffer.Length);
                return;
            }
            int pos = 0;
            while (pos < BufferSize) {
                if (mLoadSamples < mBuffL.Length) {
                    mLoader();
                }
                for (int j = 0; j < mBuffL.Length && pos < BufferSize; j++) {
                    WaveBuffer[pos] = mBuffL[j];
                    WaveBuffer[pos + 1] = mBuffR[j];
                    pos += 2;
                    mLoadSamples--;
                }
            }
            if (mStop) {
                mStopped = true;
            }
        }

        void loadVAG() {
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
            mPosition += mPackingSize * Channels;
            for (int c = 2; c < Channels; c++) {
                mFs.Position += mPackingSize;
                mPosition += mPackingSize;
            }
            if (FileSize <= Position) {
                mStopped = true;
            }
            mLoadSamples = VAG.PACKING_SAMPLES * mPackingSize >> 4;
        }
        void loadPCMStereo() {
            mWave.SetBuffer(mBuffL, mBuffR);
            mLoadSamples = mBuffL.Length;
            mPosition += mLoadSamples * mWave.SamplePerBytes;
            if (FileSize <= Position) {
                mStopped = true;
            }
        }
        void loadPCMMono() {
            mWave.SetBuffer(mBuffL);
            mLoadSamples = mBuffL.Length;
            mPosition += mLoadSamples * mWave.SamplePerBytes;
            if (FileSize <= Position) {
                mStopped = true;
            }
        }

        void openFile() {
            mWave = new RiffWave(mFilePath);
            if (mWave.IsLoadComplete) {
                mBufferSamples = ADPCM2.PACKING_SAMPLES * 4;
                mPackingSize = ADPCM2.PACKING_BYTES * 4;
                mWave.AllocateBuffer(mBufferSamples);
                switch (mWave.Channels) {
                case 1:
                    mLoader = loadPCMMono;
                    break;
                case 2:
                    mLoader = loadPCMStereo;
                    break;
                }
            } else {
                mFs = new FileStream(mFilePath, FileMode.Open);
                mBufferSamples = (mPackingSize < 128 ? 128 : mPackingSize) * VAG.PACKING_SAMPLES * 2 >> 4;
                mLoader = loadVAG;
            }
        }
        void closeFile() {
            if (null != mFs) {
                mFs.Close();
                mFs.Dispose();
                mFs = null;
            }
            if (mWave != null && mWave.IsLoadComplete) {
                mWave.Close();
                mWave = null;
            }
        }
    }
}