using System;
using System.IO;
using System.Threading;

namespace ADPCM {
    internal class WaveOut : WaveLib, IDisposable {
        string mFilePath = "";

        FileStream mFs;
        RiffWave mWave;
        RiffAdpcm mAdpcm;
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
        double[] mBuffFL;
        double[] mBuffFR;
        byte[] mEncL;
        byte[] mEncR;

        bool mStop = false;
        bool mStopped = true;

        delegate void Loader();
        Loader mLoader;

        public int VagChannels = 2;

        public long DataSize {
            get {
                if (IsRiffWave) {
                    return mWave.DataSize;
                } else if (IsRiffAdpcm) {
                    return mAdpcm.DataSize;
                } else if (null != mFs) {
                    return mFs.Length;
                } else {
                    return 1;
                }
            }
        }
        public long Position {
            get {
                if (IsRiffWave) {
                    return mWave.DataPosition;
                } else if (IsRiffAdpcm) {
                    return mAdpcm.DataPosition;
                } else if (null != mFs) {
                    return mFs.Position;
                } else {
                    return 0;
                }
            }
            set {
                if (IsRiffWave) {
                    mWave.DataPosition = value;
                } else if (IsRiffAdpcm) {
                    mAdpcm.DataPosition = value;
                } else if (null != mFs) {
                    mFs.Position = value;
                }
                mPosition = value;
            }
        }
        public bool IsRiffWave { get; private set; } = false;
        public bool IsRiffAdpcm { get; private set; } = false;

        public int PackingSize { get { return mPackingSize; } }

        public WaveOut(string filePath, int sampleRate, int packingSize = 0x800, int bits = 4) {
            mFilePath = filePath;
            mPackingSize = packingSize;
            closeFile();
            openFile();

            if (IsRiffWave) {
                mAdpcmL = new ADPCM2(16, (ADPCM2.TYPE)bits);
                mAdpcmR = new ADPCM2(16, (ADPCM2.TYPE)bits);
                mPcmL = new ADPCM2(16, (ADPCM2.TYPE)bits);
                mPcmR = new ADPCM2(16, (ADPCM2.TYPE)bits);
                mEncL = new byte[mAdpcmL.PackBytes];
                mEncR = new byte[mAdpcmR.PackBytes];
                mBufferSamples = mAdpcmL.Samples;
                mWave.AllocateBuffer(mBufferSamples);
                switch (mWave.Tag) {
                case RiffWave.TAG.INT:
                    switch (mWave.Channels) {
                    case 1:
                        mLoader = loadIntMono;
                        break;
                    case 2:
                        mLoader = loadIntStereo;
                        break;
                    }
                    break;
                case RiffWave.TAG.FLOAT:
                    mBuffFL = new double[mBufferSamples];
                    mBuffFR = new double[mBufferSamples];
                    switch (mWave.Channels) {
                    case 1:
                        mLoader = loadFloatMono;
                        break;
                    case 2:
                        mLoader = loadFloatStereo;
                        break;
                    }
                    break;
                }
                mPosition = mWave.DataPosition;
                mPackingSize = mBufferSamples * mWave.SamplePerBytes;
                mBuffL = new short[mBufferSamples];
                mBuffR = new short[mBufferSamples];
                Setup(mWave.SampleRate, mWave.Channels, mPackingSize);
            } else if (IsRiffAdpcm) {
                switch (mAdpcm.Channels) {
                case 1:
                    mLoader = loadAdpcmMono;
                    break;
                case 2:
                    mLoader = loadAdpcmStereo;
                    break;
                }
                mPosition = mAdpcm.DataPosition;
                mPackingSize = mAdpcm.PackBytes;
                mBuffL = new short[mBufferSamples];
                mBuffR = new short[mBufferSamples];
                Setup(mAdpcm.SampleRate, mAdpcm.Channels, mBufferSamples * mAdpcm.Channels * 2);
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
            mStopped = true;
            closeFile();
        }

        public void Start() {
            if (!mStopped) {
                return;
            }
            openFile();
            if (IsRiffWave) {
                mWave.DataPosition = mPosition;
            } else if (IsRiffAdpcm) {
                mWave.DataPosition = mPosition;
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
            if (1 == VagChannels) {
                Array.Copy(mBuffL, 0, mBuffR, 0, mBuffR.Length);
            }
            if (2 <= VagChannels) {
                for (int i = 0, j = 0; i < mPackingSize; i += 16, j += VAG.PACKING_SAMPLES) {
                    mFs.Read(mVagR.EncBuf, 0, mVagR.EncBuf.Length);
                    mVagR.Dec(mVagR.EncBuf);
                    Array.Copy(mVagR.DecBuf, 0, mBuffR, j, VAG.PACKING_SAMPLES);
                }
            }
            mPosition += mPackingSize * VagChannels;
            for (int c = 2; c < VagChannels; c++) {
                mFs.Position += mPackingSize;
                mPosition += mPackingSize;
            }
            if (DataSize <= Position) {
                mStopped = true;
            }
            mLoadSamples = VAG.PACKING_SAMPLES * mPackingSize >> 4;
        }

        void loadAdpcmStereo() {
            mAdpcm.SetBufferCh2(mBuffL, mBuffR);
            mLoadSamples = mBuffL.Length;
            mPosition += mAdpcm.PackBytes;
            if (DataSize <= Position) {
                mStopped = true;
            }
        }
        void loadAdpcmMono() {
            mAdpcm.SetBufferCh1(mBuffL);
            mLoadSamples = mBuffL.Length;
            mPosition += mAdpcm.PackBytes;
            if (DataSize <= Position) {
                mStopped = true;
            }
        }

        void loadIntStereo() {
            mWave.SetBufferInt(mBuffL, mBuffR);
            mAdpcmL.Encode(mBuffL, mEncL);
            mAdpcmR.Encode(mBuffR, mEncR);
            mPcmL.Decode(mBuffL, mEncL);
            mPcmR.Decode(mBuffR, mEncR);
            mLoadSamples = mBuffL.Length;
            mPosition += mLoadSamples * mWave.SamplePerBytes;
            if (DataSize <= Position) {
                mStopped = true;
            }
        }
        void loadIntMono() {
            mWave.SetBufferInt(mBuffL);
            mAdpcmL.Encode(mBuffL, mEncL);
            mPcmL.Decode(mBuffL, mEncL);
            mLoadSamples = mBuffL.Length;
            mPosition += mLoadSamples * mWave.SamplePerBytes;
            if (DataSize <= Position) {
                mStopped = true;
            }
        }
        void loadFloatStereo() {
            mWave.SetBufferFloat(mBuffFL, mBuffFR);
            for(int i=0; i< mBuffFL.Length; i++) {
                var l = mBuffFL[i];
                var r = mBuffFR[i];
                if (l < -1.0) {
                    l = -1.0;
                }
                if (1.0 < l) {
                    l = 1.0;
                }
                if (r < -1.0) {
                    r = -1.0;
                }
                if (1.0 < r) {
                    r = 1.0;
                }
                mBuffL[i] = (short)(l * 32767);
                mBuffR[i] = (short)(r * 32767);
            }
            mAdpcmL.Encode(mBuffL, mEncL);
            mAdpcmR.Encode(mBuffR, mEncR);
            mPcmL.Decode(mBuffL, mEncL);
            mPcmR.Decode(mBuffR, mEncR);
            mLoadSamples = mBuffL.Length;
            mPosition += mLoadSamples * mWave.SamplePerBytes;
            if (DataSize <= Position) {
                mStopped = true;
            }
        }
        void loadFloatMono() {
            mWave.SetBufferFloat(mBuffFL);
            for (int i = 0; i < mBuffFL.Length; i++) {
                var l = mBuffFL[i];
                if (l < -1.0) {
                    l = -1.0;
                }
                if (1.0 < l) {
                    l = 1.0;
                }
                mBuffL[i] = (short)(l * 32767);
            }
            mAdpcmL.Encode(mBuffL, mEncL);
            mPcmL.Decode(mBuffL, mEncL);
            mLoadSamples = mBuffL.Length;
            mPosition += mLoadSamples * mWave.SamplePerBytes;
            if (DataSize <= Position) {
                mStopped = true;
            }
        }

        void openFile() {
            closeFile();
            mWave = new RiffWave(mFilePath);
            if (mWave.IsLoadComplete) {
                mWave.AllocateBuffer(mBufferSamples);
                IsRiffWave = true;
            } else {
                mAdpcm = new RiffAdpcm(mFilePath);
                if (mAdpcm.IsLoadComplete) {
                    IsRiffAdpcm = true;
                    mBufferSamples = mAdpcm.PackSamples;
                } else {
                    mFs = new FileStream(mFilePath, FileMode.Open);
                    mBufferSamples = (mPackingSize < 128 ? 128 : mPackingSize) * VAG.PACKING_SAMPLES * 2 >> 4;
                }
            }
        }
        void closeFile() {
            if (null != mFs) {
                mFs.Close();
                mFs.Dispose();
                mFs = null;
            }
            if (null != mWave) {
                mWave.Close();
                mWave = null;
            }
            if (null != mAdpcm) {
                mAdpcm.Close();
                mAdpcm = null;
            }
            IsRiffWave = false;
            IsRiffAdpcm = false;
        }
    }
}