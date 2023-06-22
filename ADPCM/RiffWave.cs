using System;
using System.IO;

namespace ADPCM {
    internal class RiffWave : RiffFile {
        public enum TYPE {
            INT8_CH1 = 0x010801,
            INT8_CH2 = 0x010802,
            INT16_CH1 = 0x011001,
            INT16_CH2 = 0x011002,
            INT24_CH1 = 0x011801,
            INT24_CH2 = 0x011802,
            INT32_CH1 = 0x012001,
            INT32_CH2 = 0x012002,
            FLOAT24_CH1 = 0x031801,
            FLOAT24_CH2 = 0x031802,
            FLOAT32_CH1 = 0x032001,
            FLOAT32_CH2 = 0x032002,
            FLOAT64_CH1 = 0x034001,
            FLOAT64_CH2 = 0x034002
        }

        public TYPE Type { get; private set; }
        public int SampleRate { get; private set; }

        long mPosData = 0;
        long mPosFmt = 0;
        byte[] mBuffer;
        byte[] m4byte = new byte[4];
        ADPCM2 mAdpcmL;
        ADPCM2 mAdpcmR;
        ADPCM2 mPcmL;
        ADPCM2 mPcmR;
        byte[] mEncL;
        byte[] mEncR;

        public RiffWave(TYPE type, int sampleRate) {
            Type = type;
            SampleRate = sampleRate;
        }

        public RiffWave(string path) : base(path) {}

        public void AllocateBuffer(int samples) {
            var flag = (int)Type;
            mBuffer = new byte[samples * (flag & 0xFF00) * (flag & 0xFF) >> 11];
            mAdpcmL = new ADPCM2(samples);
            mAdpcmR = new ADPCM2(samples);
            mPcmL = new ADPCM2(samples);
            mPcmR = new ADPCM2(samples);
            var packingSize = samples * ADPCM2.PACKING_BYTES / ADPCM2.PACKING_SAMPLES;
            mEncL = new byte[packingSize];
            mEncR = new byte[packingSize];
        }

        protected override bool CheckType(string type) {
            return "WAVE" == type;
        }

        protected override bool LoadChunk(string type, int size) {
            switch (type) {
            case "data":
                mPosData = mFs.Position;
                break;
            case "fmt ":
                mPosFmt = mFs.Position;
                break;
            }
            return false;
        }

        protected override bool LoadList(string type, int size) {
            return false;
        }

        protected override void LoadComplete() {
            var br = new BinaryReader(mFs);
            /*** Load fmt ***/
            mFs.Position = mPosFmt;
            var tag = br.ReadUInt16();
            var channels = br.ReadUInt16();
            SampleRate = br.ReadInt32();
            br.ReadInt32();
            br.ReadUInt16();
            var bits = br.ReadUInt16();

            /*** Check format ***/
            Type = (TYPE)(tag << 16 | bits << 8 | channels);
            IsLoadComplete = Enum.IsDefined(typeof(TYPE), Type);
            if (!IsLoadComplete) {
                return;
            }

            /*** Setting data chunk position ***/
            mFs.Position = mPosData;

            /*** Setting the buffer loading function ***/
            switch (Type) {
            case TYPE.INT8_CH1:
                SetBuffer = SetBufferInt8Ch1;
                break;
            case TYPE.INT8_CH2:
                SetBuffer = SetBufferInt8Ch2;
                break;
            case TYPE.INT16_CH1:
                SetBuffer = SetBufferInt16Ch1;
                break;
            case TYPE.INT16_CH2:
                SetBuffer = SetBufferInt16Ch2;
                break;
            case TYPE.INT24_CH1:
                SetBuffer = SetBufferInt24Ch1;
                break;
            case TYPE.INT24_CH2:
                SetBuffer = SetBufferInt24Ch2;
                break;
            case TYPE.INT32_CH1:
                SetBuffer = SetBufferInt32Ch1;
                break;
            case TYPE.INT32_CH2:
                SetBuffer = SetBufferInt32Ch2;
                break;
            case TYPE.FLOAT24_CH1:
                SetBuffer = SetBufferFloat24Ch1;
                break;
            case TYPE.FLOAT24_CH2:
                SetBuffer = SetBufferFloat24Ch2;
                break;
            case TYPE.FLOAT32_CH1:
                SetBuffer = SetBufferFloat32Ch1;
                break;
            case TYPE.FLOAT32_CH2:
                SetBuffer = SetBufferFloat32Ch2;
                break;
            case TYPE.FLOAT64_CH1:
                SetBuffer = SetBufferFloat64Ch1;
                break;
            case TYPE.FLOAT64_CH2:
                SetBuffer = SetBufferFloat64Ch2;
                break;
            }
        }

        public delegate void DSetBuffer(short[] left, short[] right);
        public DSetBuffer SetBuffer;
        void SetBufferInt8Ch1(short[] left, short[] right) {
            mFs.Read(mBuffer, 0, mBuffer.Length);
            for (int i=0; i< left.Length; i++) {
                left[i] = (short)((mBuffer[i] - 128) * 256);
                right[i] = left[i];
            }
        }
        void SetBufferInt8Ch2(short[] left, short[] right) {
            mFs.Read(mBuffer, 0, mBuffer.Length);
            for (int i = 0, j = 0; i < left.Length; i++, j += 2) {
                left[i] = (short)((mBuffer[j] - 128) * 256);
                right[i] = (short)((mBuffer[j + 1] - 128) * 256);
            }
        }
        void SetBufferInt16Ch1(short[] left, short[] right) {
            mFs.Read(mBuffer, 0, mBuffer.Length);
            for (int i = 0, j = 0; i < left.Length; i++, j += 2) {
                left[i] = BitConverter.ToInt16(mBuffer, j);
                right[i] = left[i];
            }
        }
        void SetBufferInt16Ch2(short[] left, short[] right) {
            mFs.Read(mBuffer, 0, mBuffer.Length);
            for (int i = 0, j = 0; i < left.Length; i++, j += 4) {
                left[i] = BitConverter.ToInt16(mBuffer, j);
                right[i] = BitConverter.ToInt16(mBuffer, j + 2);
            }
            mAdpcmL.Encode(left, mEncL);
            mAdpcmR.Encode(right, mEncR);
            mPcmL.Decode(left, mEncL);
            mPcmR.Decode(right, mEncR);
        }
        void SetBufferInt24Ch1(short[] left, short[] right) {
            mFs.Read(mBuffer, 0, mBuffer.Length);
            for (int i = 0, j = 0; i < left.Length; i++, j += 3) {
                left[i] = BitConverter.ToInt16(mBuffer, j + 1);
                right[i] = left[i];
            }
        }
        void SetBufferInt24Ch2(short[] left, short[] right) {
            mFs.Read(mBuffer, 0, mBuffer.Length);
            for (int i = 0, j = 0; i < left.Length; i++, j += 6) {
                left[i] = BitConverter.ToInt16(mBuffer, j + 1);
                right[i] = BitConverter.ToInt16(mBuffer, j + 4);
            }
        }
        void SetBufferInt32Ch1(short[] left, short[] right) {
            mFs.Read(mBuffer, 0, mBuffer.Length);
            for (int i = 0, j = 0; i < left.Length; i++, j += 4) {
                left[i] = BitConverter.ToInt16(mBuffer, j + 2);
                right[i] = left[i];
            }
        }
        void SetBufferInt32Ch2(short[] left, short[] right) {
            mFs.Read(mBuffer, 0, mBuffer.Length);
            for (int i = 0, j = 0; i < left.Length; i++, j += 8) {
                left[i] = BitConverter.ToInt16(mBuffer, j + 2);
                right[i] = BitConverter.ToInt16(mBuffer, j + 6);
            }
        }
        void SetBufferFloat24Ch1(short[] left, short[] right) {
            mFs.Read(mBuffer, 0, mBuffer.Length);
            m4byte[0] = 0;
            for (int i = 0, j = 0; i < left.Length; i++, j += 3) {
                m4byte[1] = mBuffer[j];
                m4byte[2] = mBuffer[j + 1];
                m4byte[3] = mBuffer[j + 2];
                var mono = BitConverter.ToSingle(m4byte, 0);
                if (mono < -1.0f) {
                    mono = -1.0f;
                }
                if (1.0f < mono) {
                    mono = 1.0f;
                }
                left[i] = (short)(mono * 32767);
                right[i] = left[i];
            }
        }
        void SetBufferFloat24Ch2(short[] left, short[] right) {
            mFs.Read(mBuffer, 0, mBuffer.Length);
            m4byte[0] = 0;
            for (int i = 0, j = 0; i < left.Length; i++, j += 6) {
                m4byte[1] = mBuffer[j];
                m4byte[2] = mBuffer[j + 1];
                m4byte[3] = mBuffer[j + 2];
                var l = BitConverter.ToSingle(m4byte, 0);
                m4byte[1] = mBuffer[j + 3];
                m4byte[2] = mBuffer[j + 4];
                m4byte[3] = mBuffer[j + 5];
                var r = BitConverter.ToSingle(m4byte, 0);
                if (l < -1.0f) {
                    l = -1.0f;
                }
                if (1.0f < l) {
                    l = 1.0f;
                }
                if (r < -1.0f) {
                    r = -1.0f;
                }
                if (1.0f < r) {
                    r = 1.0f;
                }
                left[i] = (short)(l * 32767);
                right[i] = (short)(r * 32767);
            }
        }
        void SetBufferFloat32Ch1(short[] left, short[] right) {
            mFs.Read(mBuffer, 0, mBuffer.Length);
            for (int i = 0, j = 0; i < left.Length; i++, j += 4) {
                var mono = BitConverter.ToSingle(mBuffer, j);
                if (mono < -1.0f) {
                    mono = -1.0f;
                }
                if (1.0f < mono) {
                    mono = 1.0f;
                }
                left[i] = (short)(mono * 32767);
                right[i] = left[i];
            }
        }
        void SetBufferFloat32Ch2(short[] left, short[] right) {
            mFs.Read(mBuffer, 0, mBuffer.Length);
            for (int i = 0, j = 0; i < left.Length; i++, j += 8) {
                var l = BitConverter.ToSingle(mBuffer, j);
                var r = BitConverter.ToSingle(mBuffer, j + 4);
                if (l < -1.0f) {
                    l = -1.0f;
                }
                if (1.0f < l) {
                    l = 1.0f;
                }
                if (r < -1.0f) {
                    r = -1.0f;
                }
                if (1.0f < r) {
                    r = 1.0f;
                }
                left[i] = (short)(l * 32767);
                right[i] = (short)(r * 32767);
            }
            mAdpcmL.Encode(left, mEncL);
            mAdpcmR.Encode(right, mEncR);
            mPcmL.Decode(left, mEncL);
            mPcmR.Decode(right, mEncR);
        }
        void SetBufferFloat64Ch1(short[] left, short[] right) {
            mFs.Read(mBuffer, 0, mBuffer.Length);
            for (int i = 0, j = 0; i < left.Length; i++, j += 8) {
                var mono = BitConverter.ToDouble(mBuffer, j);
                if (mono < -1.0) {
                    mono = -1.0;
                }
                if (1.0 < mono) {
                    mono = 1.0;
                }
                left[i] = (short)(mono * 32767);
                right[i] = left[i];
            }
        }
        void SetBufferFloat64Ch2(short[] left, short[] right) {
            mFs.Read(mBuffer, 0, mBuffer.Length);
            for (int i = 0, j = 0; i < left.Length; i++, j += 16) {
                var l = BitConverter.ToDouble(mBuffer, j);
                var r = BitConverter.ToDouble(mBuffer, j + 8);
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
                left[i] = (short)(l * 32767);
                right[i] = (short)(r * 32767);
            }
        }
    }
}
