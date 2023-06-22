using System;
using System.IO;
using System.Threading;

namespace ADPCM {
    internal class WaveOut : WaveLib, IDisposable {
        string mFilePath = "";

        FileStream mFs;
        RiffWave mWave;
        VAG mVagL = new VAG();
        VAG mVagR = new VAG();
        ADPCM2 mAdpcmL;
        ADPCM2 mAdpcmR;
        ADPCM2 mPcmL;
        ADPCM2 mPcmR;

        int mPackingSize;
        int mBufferSamples;
        int mLoadSamples = 0;
        long mPosition = 0;
        short[] mBuffL;
        short[] mBuffR;
        byte[] mEncL;
        byte[] mEncR;

        bool mStop = false;
        bool mStopped = true;

        delegate void Loader();
        Loader mLoader;

        public new int Channels = 2;

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

            if (mWave.IsLoadComplete) {
                mAdpcmL = new ADPCM2(16, ADPCM2.TYPE.BIT3);
                mAdpcmR = new ADPCM2(16, ADPCM2.TYPE.BIT3);
                mPcmL = new ADPCM2(16, ADPCM2.TYPE.BIT3);
                mPcmR = new ADPCM2(16, ADPCM2.TYPE.BIT3);
                mEncL = new byte[mAdpcmL.PackBytes];
                mEncR = new byte[mAdpcmR.PackBytes];
                mBufferSamples = mAdpcmL.Samples;
                mPackingSize = mBufferSamples * mWave.SamplePerBytes;
                mWave.AllocateBuffer(mBufferSamples);
                switch (mWave.Channels) {
                case 1:
                    mLoader = loadPCMMono;
                    break;
                case 2:
                    mLoader = loadPCMStereo;
                    break;
                }
                mPosition = mWave.Position;
                mBuffL = new short[mBufferSamples];
                mBuffR = new short[mBufferSamples];
                Setup(mWave.SampleRate, 2, mPackingSize);
            } else {
                mLoader = loadVAG;
                mPosition = 0;
                mBuffL = new short[mBufferSamples];
                mBuffR = new short[mBufferSamples];
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
                mFs.Read(mVagL.EncBuf, 0, mVagL.EncBuf.Length);
                mVagL.Dec(mVagL.EncBuf);
                Array.Copy(mVagL.DecBuf, 0, mBuffL, j, VAG.PACKING_SAMPLES);
            }
            if (1 == Channels) {
                Array.Copy(mBuffL, 0, mBuffR, 0, mBuffR.Length);
            }
            if (2 <= Channels) {
                for (int i = 0, j = 0; i < mPackingSize; i += 16, j += VAG.PACKING_SAMPLES) {
                    mFs.Read(mVagR.EncBuf, 0, mVagR.EncBuf.Length);
                    mVagR.Dec(mVagR.EncBuf);
                    Array.Copy(mVagR.DecBuf, 0, mBuffR, j, VAG.PACKING_SAMPLES);
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
            mAdpcmL.Encode(mBuffL, mEncL);
            mAdpcmR.Encode(mBuffR, mEncR);
            mPcmL.Decode(mBuffL, mEncL);
            mPcmR.Decode(mBuffR, mEncR);
            mLoadSamples = mBuffL.Length;
            mPosition += mLoadSamples * mWave.SamplePerBytes;
            if (FileSize <= Position) {
                mStopped = true;
            }
        }
        void loadPCMMono() {
            mWave.SetBuffer(mBuffL);
            mAdpcmL.Encode(mBuffL, mEncL);
            mPcmL.Decode(mBuffL, mEncL);
            Array.Copy(mBuffL, 0, mBuffR, 0, mBuffR.Length);
            mLoadSamples = mBuffL.Length;
            mPosition += mLoadSamples * mWave.SamplePerBytes;
            if (FileSize <= Position) {
                mStopped = true;
            }
        }

        void openFile() {
            mWave = new RiffWave(mFilePath);
            if (mWave.IsLoadComplete) {
                mWave.AllocateBuffer(mBufferSamples);
            } else {
                mFs = new FileStream(mFilePath, FileMode.Open);
                mBufferSamples = (mPackingSize < 128 ? 128 : mPackingSize) * VAG.PACKING_SAMPLES * 2 >> 4;
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