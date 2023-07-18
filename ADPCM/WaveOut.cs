using System;
using System.IO;
using System.Threading;

namespace ADPCM {
    internal class WaveOut : WaveLib, IDisposable {
        string mFilePath = "";

        FileStream mFsVag;
        RiffWave mWave;
        RiffAdpcm mAdpcm;

        VAG mVagL = new VAG();
        VAG mVagR = new VAG();
        ADPCM2 mAdpcmEL;
        ADPCM2 mAdpcmER;
        ADPCM2 mAdpcmDL;
        ADPCM2 mAdpcmDR;

        int mLoadedSamples = 0;
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
        public double Volume = 1.0;

        public int PackingSize { get; private set; }
        public long DataSize { get; private set; }
        public long Position {
            get { return mPosition; }
            set {
                if (IsRiffWave) {
                    mWave.DataPosition = value;
                } else if (IsRiffAdpcm) {
                    mAdpcm.DataPosition = value;
                    mAdpcmDL.Clear();
                    mAdpcmDR.Clear();
                } else if (null != mFsVag) {
                    mFsVag.Position = value;
                }
                mPosition = value;
            }
        }
        public bool IsRiffWave { get; private set; } = false;
        public bool IsRiffAdpcm { get; private set; } = false;
        public int Bits {
            get {
                if (IsRiffWave) {
                    return mWave.Bits;
                } else if (IsRiffAdpcm) {
                    return (int)mAdpcm.Type;
                } else {
                    return 4;
                }
            }
        }

        public WaveOut(string filePath, int sampleRate, int packingSize = 0x800, int bits = 4) {
            mFilePath = filePath;
            closeFile();
            openFile();

            if (IsRiffWave) {
                mAdpcmEL = new ADPCM2((ADPCM2.TYPE)bits, 128);
                mAdpcmER = new ADPCM2((ADPCM2.TYPE)bits, 128);
                mAdpcmDL = new ADPCM2((ADPCM2.TYPE)bits, 128);
                mAdpcmDR = new ADPCM2((ADPCM2.TYPE)bits, 128);
                mEncL = new byte[mAdpcmEL.PackBytes];
                mEncR = new byte[mAdpcmER.PackBytes];
                var bufferSamples = mAdpcmEL.Samples;
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
                    mBuffFL = new double[bufferSamples];
                    mBuffFR = new double[bufferSamples];
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
                PackingSize = bufferSamples * mWave.SamplePerBytes;
                mBuffL = new short[bufferSamples];
                mBuffR = new short[bufferSamples];
                Setup(mWave.SampleRate, mWave.Channels, bufferSamples * mWave.Channels * 2);
            } else if (IsRiffAdpcm) {
                mAdpcmDL = new ADPCM2(mAdpcm.Type, mAdpcm.Packes);
                mAdpcmDR = new ADPCM2(mAdpcm.Type, mAdpcm.Packes);
                mEncL = new byte[mAdpcmDL.PackBytes];
                mEncR = new byte[mAdpcmDR.PackBytes];
                var bufferSamples = mAdpcmDL.Samples * 64;
                switch (mAdpcm.Channels) {
                case 1:
                    mLoader = loadAdpcmMono;
                    break;
                case 2:
                    mLoader = loadAdpcmStereo;
                    break;
                }
                mPosition = mAdpcm.DataPosition;
                PackingSize = mAdpcmDL.PackBytes * mAdpcm.Channels * 64;
                mBuffL = new short[bufferSamples];
                mBuffR = new short[bufferSamples];
                Setup(mAdpcm.SampleRate, mAdpcm.Channels, bufferSamples * mAdpcm.Channels * 2);
            } else {
                mLoader = loadVAG;
                mPosition = 0;
                PackingSize = packingSize;
                var bufferSamples = (PackingSize < 128 ? 128 : PackingSize) * VAG.UNIT_SAMPLES * 2 >> 4;
                mEncL = new byte[VAG.UNIT_BYTES];
                mEncR = new byte[VAG.UNIT_BYTES];
                mBuffL = new short[bufferSamples];
                mBuffR = new short[bufferSamples];
                Setup(sampleRate, 2, bufferSamples);
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
                mAdpcm.DataPosition = mPosition;
            } else {
                mFsVag.Position = mPosition;
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
            switch (Channels) {
            case 1:
                while (pos < BufferSamples) {
                    if (mLoadedSamples < mBuffL.Length) {
                        mLoader();
                    }
                    for (int j = 0; j < mBuffL.Length && pos < BufferSamples; j++) {
                        WaveBuffer[pos] = (short)(mBuffL[j] * Volume);
                        pos++;
                        mLoadedSamples--;
                    }
                }
                break;
            case 2:
                while (pos < BufferSamples) {
                    if (mLoadedSamples < mBuffL.Length) {
                        mLoader();
                    }
                    for (int j = 0; j < mBuffL.Length && pos < BufferSamples; j++) {
                        WaveBuffer[pos] = (short)(mBuffL[j] * Volume);
                        WaveBuffer[pos + 1] = (short)(mBuffR[j] * Volume);
                        pos += 2;
                        mLoadedSamples--;
                    }
                }
                break;
            }
            if (mStop) {
                mStopped = true;
            }
        }

        void loadVAG() {
            for (int b = 0, s = 0; b < PackingSize; b += VAG.UNIT_BYTES, s += VAG.UNIT_SAMPLES) {
                mFsVag.Read(mEncL, 0, mEncL.Length);
                mVagL.Decode(mEncL, mBuffL, s);
            }
            mPosition += PackingSize;
            if (1 == VagChannels) {
                Array.Copy(mBuffL, 0, mBuffR, 0, mBuffR.Length);
            }
            if (2 <= VagChannels) {
                for (int b = 0, s = 0; b < PackingSize; b += VAG.UNIT_BYTES, s += VAG.UNIT_SAMPLES) {
                    mFsVag.Read(mEncR, 0, mEncR.Length);
                    mVagR.Decode(mEncR, mBuffR, s);
                }
                mPosition += PackingSize;
            }
            for (int c = 2; c < VagChannels; c++) {
                mFsVag.Position += PackingSize;
                mPosition += PackingSize;
            }
            if (DataSize <= Position) {
                mStopped = true;
            }
            mLoadedSamples = VAG.UNIT_SAMPLES * PackingSize >> 4;
        }

        void loadAdpcmStereo() {
            for (int s = 0; s < mBuffL.Length; s += mAdpcmDL.Samples) {
                mAdpcm.SetBufferCh2(mEncL, mEncR);
                mAdpcmDL.Decode(mBuffL, mEncL, s);
                mAdpcmDR.Decode(mBuffR, mEncR, s);
                mLoadedSamples = mBuffL.Length;
                mPosition += mAdpcmDL.PackBytes * 2;
                if (DataSize <= Position) {
                    mStopped = true;
                }
            }
            for (int i = 0; i < mBuffL.Length; i++) {
                var l = mBuffL[i] + (mBuffR[i] << RiffAdpcm.JOINT_STEREO);
                var r = mBuffL[i] - (mBuffR[i] << RiffAdpcm.JOINT_STEREO);
                if (l < -32768) {
                    l = -32768;
                }
                if (32767 < l) {
                    l = 32767;
                }
                if (r < -32768) {
                    r = -32768;
                }
                if (32767 < r) {
                    r = 32767;
                }
                mBuffL[i] = (short)l;
                mBuffR[i] = (short)r;
            }
        }
        void loadAdpcmMono() {
            for (int s = 0; s < mBuffL.Length; s += mAdpcmDL.Samples) {
                mAdpcm.SetBufferCh1(mEncL);
                mAdpcmDL.Decode(mBuffL, mEncL, s);
                mLoadedSamples = mBuffL.Length;
                mPosition += mAdpcmDL.PackBytes;
                if (DataSize <= Position) {
                    mStopped = true;
                }
            }
        }

        void loadIntStereo() {
            mWave.SetBufferInt(mBuffL, mBuffR);
            for (int i = 0; i < mBuffL.Length; i++) {
                var m = (short)((mBuffL[i] + mBuffR[i]) >> 1);
                var s = (short)((mBuffL[i] - mBuffR[i]) >> (RiffAdpcm.JOINT_STEREO+1));
                mBuffL[i] = m;
                mBuffR[i] = s;
            }
            mAdpcmEL.Encode(mBuffL, mEncL);
            mAdpcmER.Encode(mBuffR, mEncR);
            mAdpcmDL.Decode(mBuffL, mEncL);
            mAdpcmDR.Decode(mBuffR, mEncR);
            for (int i = 0; i < mBuffL.Length; i++) {
                var l = mBuffL[i] + (mBuffR[i] << RiffAdpcm.JOINT_STEREO);
                var r = mBuffL[i] - (mBuffR[i] << RiffAdpcm.JOINT_STEREO);
                if (l < -32768) {
                    l = -32768;
                }
                if (32767 < l) {
                    l = 32767;
                }
                if (r < -32768) {
                    r = -32768;
                }
                if (32767 < r) {
                    r = 32767;
                }
                mBuffL[i] = (short)l;
                mBuffR[i] = (short)r;
            }
            mLoadedSamples = mBuffL.Length;
            mPosition += mLoadedSamples * mWave.SamplePerBytes;
            if (DataSize <= Position) {
                mStopped = true;
            }
        }
        void loadIntMono() {
            mWave.SetBufferInt(mBuffL);
            mAdpcmEL.Encode(mBuffL, mEncL);
            mAdpcmDL.Decode(mBuffL, mEncL);
            mLoadedSamples = mBuffL.Length;
            mPosition += mLoadedSamples * mWave.SamplePerBytes;
            if (DataSize <= Position) {
                mStopped = true;
            }
        }
        void loadFloatStereo() {
            mWave.SetBufferFloat(mBuffFL, mBuffFR);
            for (int i = 0; i < mBuffFL.Length; i++) {
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
            mAdpcmEL.Encode(mBuffL, mEncL);
            mAdpcmER.Encode(mBuffR, mEncR);
            mAdpcmDL.Decode(mBuffL, mEncL);
            mAdpcmDR.Decode(mBuffR, mEncR);
            mLoadedSamples = mBuffL.Length;
            mPosition += mLoadedSamples * mWave.SamplePerBytes;
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
            mAdpcmEL.Encode(mBuffL, mEncL);
            mAdpcmDL.Decode(mBuffL, mEncL);
            mLoadedSamples = mBuffL.Length;
            mPosition += mLoadedSamples * mWave.SamplePerBytes;
            if (DataSize <= Position) {
                mStopped = true;
            }
        }

        void openFile() {
            closeFile();
            mWave = new RiffWave(mFilePath);
            if (mWave.IsLoadComplete) {
                if (null != mBuffL) {
                    mWave.AllocateBuffer(mBuffL.Length);
                }
                DataSize = mWave.DataSize;
                IsRiffWave = true;
            } else {
                mAdpcm = new RiffAdpcm(mFilePath);
                if (mAdpcm.IsLoadComplete) {
                    DataSize = mAdpcm.DataSize;
                    IsRiffAdpcm = true;
                } else {
                    mFsVag = new FileStream(mFilePath, FileMode.Open);
                    DataSize = mFsVag.Length;
                }
            }
        }
        void closeFile() {
            if (null != mFsVag) {
                mFsVag.Close();
                mFsVag.Dispose();
                mFsVag = null;
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