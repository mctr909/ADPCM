using System;
using System.IO;

class RiffWave : RiffFile {
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
    public int Channels { get; private set; }
    public int Bits { get; private set; }
    public int SamplePerBytes { get { return Bits * Channels >> 3; } }

    public int Samples { get; private set; } = 0;

    long mPosData = 0;
    long mSizeData = 0;
    long mPosFmt = 0;
    byte[] mBuffer;
    byte[] m4byte = new byte[4];

    public RiffWave(string path, TYPE type, int sampleRate) {
        mFs = new FileStream(path, FileMode.Create);
        mFs.Write(new byte[44], 0, 12);

        Type = type;
        Channels = (int)type & 0xFF;
        Bits = ((int)type & 0xFF00) >> 8;
        SampleRate = sampleRate;

        /*** Setting the writing function ***/
        WriteInt = WriteIntNop;
        WriteFloat = WriteFloatNop;
        switch (Type) {
        case TYPE.INT8_CH1:
            WriteInt = WriteInt8Ch1;
            break;
        case TYPE.INT8_CH2:
            WriteInt = WriteInt8Ch2;
            break;
        case TYPE.INT16_CH1:
            WriteInt = WriteInt16Ch1;
            break;
        case TYPE.INT16_CH2:
            WriteInt = WriteInt16Ch2;
            break;
        case TYPE.INT24_CH1:
            WriteInt = WriteInt24Ch1;
            break;
        case TYPE.INT24_CH2:
            WriteInt = WriteInt24Ch2;
            break;
        case TYPE.INT32_CH1:
            WriteInt = WriteInt32Ch1;
            break;
        case TYPE.INT32_CH2:
            WriteInt = WriteInt32Ch2;
            break;
        case TYPE.FLOAT24_CH1:
            WriteFloat = WriteFloat24Ch1;
            break;
        case TYPE.FLOAT24_CH2:
            WriteFloat = WriteFloat24Ch2;
            break;
        case TYPE.FLOAT32_CH1:
            WriteFloat = WriteFloat32Ch1;
            break;
        case TYPE.FLOAT32_CH2:
            WriteFloat = WriteFloat32Ch2;
            break;
        case TYPE.FLOAT64_CH1:
            WriteFloat = WriteFloat64Ch1;
            break;
        case TYPE.FLOAT64_CH2:
            WriteFloat = WriteFloat64Ch2;
            break;
        default:
            break;
        }
        SetBufferInt = SetBufferIntNop;
        SetBufferFloat = SetBufferFloatNop;
    }

    public RiffWave(string path) : base(path) { }

    public void AllocateBuffer(int samples) {
        mBuffer = new byte[samples * Bits * Channels >> 3];
    }

    public override void Close() {
        if (null == mFs) {
            return;
        }
        if (!mFs.CanWrite) {
            base.Close();
            return;
        }

        mFs.Position = 0;
        var bw = new BinaryWriter(mFs);

        bw.Write(new char[] { 'R', 'I', 'F', 'F' });
        bw.Write((int)mFs.Length - 8);
        bw.Write(new char[] { 'W', 'A', 'V', 'E' });

        bw.Write(new char[] { 'f', 'm', 't', ' ' });
        bw.Write((int)16);
        bw.Write((ushort)((int)Type >> 16));
        bw.Write((ushort)Channels);
        bw.Write(SampleRate);
        bw.Write(SamplePerBytes * SampleRate);
        bw.Write((ushort)SamplePerBytes);
        bw.Write((ushort)Bits);

        bw.Write(new char[] { 'd', 'a', 't', 'a' });
        bw.Write(SamplePerBytes * Samples);

        base.Close();
    }

    protected override bool CheckType(string type) {
        return "WAVE" == type;
    }

    protected override bool LoadChunk(string type, int size) {
        switch (type) {
        case "data":
            mPosData = mFs.Position;
            mSizeData = size;
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
        Channels = br.ReadUInt16();
        SampleRate = br.ReadInt32();
        br.ReadInt32();
        br.ReadUInt16();
        Bits = br.ReadUInt16();

        /*** Check format ***/
        Type = (TYPE)(tag << 16 | Bits << 8 | Channels);
        IsLoadComplete = Enum.IsDefined(typeof(TYPE), Type);
        if (!IsLoadComplete) {
            return;
        }

        /*** Set samples count ***/
        var samplePerBytes = Channels * Bits >> 3;
        Samples = (int)(mSizeData / samplePerBytes);

        /*** Setting data chunk position ***/
        mFs.Position = mPosData;

        /*** Setting the buffer loading function ***/
        SetBufferInt = SetBufferIntNop;
        SetBufferFloat = SetBufferFloatNop;
        switch (Type) {
        case TYPE.INT8_CH1:
            SetBufferInt = SetBufferInt8Ch1;
            break;
        case TYPE.INT8_CH2:
            SetBufferInt = SetBufferInt8Ch2;
            break;
        case TYPE.INT16_CH1:
            SetBufferInt = SetBufferInt16Ch1;
            break;
        case TYPE.INT16_CH2:
            SetBufferInt = SetBufferInt16Ch2;
            break;
        case TYPE.INT24_CH1:
            SetBufferInt = SetBufferInt24Ch1;
            break;
        case TYPE.INT24_CH2:
            SetBufferInt = SetBufferInt24Ch2;
            break;
        case TYPE.INT32_CH1:
            SetBufferInt = SetBufferInt32Ch1;
            break;
        case TYPE.INT32_CH2:
            SetBufferInt = SetBufferInt32Ch2;
            break;
        case TYPE.FLOAT24_CH1:
            SetBufferFloat = SetBufferFloat24Ch1;
            break;
        case TYPE.FLOAT24_CH2:
            SetBufferFloat = SetBufferFloat24Ch2;
            break;
        case TYPE.FLOAT32_CH1:
            SetBufferFloat = SetBufferFloat32Ch1;
            break;
        case TYPE.FLOAT32_CH2:
            SetBufferFloat = SetBufferFloat32Ch2;
            break;
        case TYPE.FLOAT64_CH1:
            SetBufferFloat = SetBufferFloat64Ch1;
            break;
        case TYPE.FLOAT64_CH2:
            SetBufferFloat = SetBufferFloat64Ch2;
            break;
        default:
            break;
        }
        WriteInt = WriteIntNop;
        WriteFloat = WriteFloatNop;
    }

    public delegate void DSetBuffer(short[] left, short[] right = null);
    public delegate void DDSetBuffer(double[] left, double[] right = null);
    public DSetBuffer SetBufferInt;
    public DDSetBuffer SetBufferFloat;
    void SetBufferIntNop(short[] left, short[] right) { }
    void SetBufferInt8Ch1(short[] left, short[] right) {
        mFs.Read(mBuffer, 0, mBuffer.Length);
        for (int i = 0; i < left.Length; i++) {
            left[i] = (short)((mBuffer[i] - 128) * 256);
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
        }
    }
    void SetBufferInt16Ch2(short[] left, short[] right) {
        mFs.Read(mBuffer, 0, mBuffer.Length);
        for (int i = 0, j = 0; i < left.Length; i++, j += 4) {
            left[i] = BitConverter.ToInt16(mBuffer, j);
            right[i] = BitConverter.ToInt16(mBuffer, j + 2);
        }
    }
    void SetBufferInt24Ch1(short[] left, short[] right) {
        mFs.Read(mBuffer, 0, mBuffer.Length);
        for (int i = 0, j = 0; i < left.Length; i++, j += 3) {
            left[i] = BitConverter.ToInt16(mBuffer, j + 1);
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
        }
    }
    void SetBufferInt32Ch2(short[] left, short[] right) {
        mFs.Read(mBuffer, 0, mBuffer.Length);
        for (int i = 0, j = 0; i < left.Length; i++, j += 8) {
            left[i] = BitConverter.ToInt16(mBuffer, j + 2);
            right[i] = BitConverter.ToInt16(mBuffer, j + 6);
        }
    }
    void SetBufferFloatNop(double[] left, double[] right) { }
    void SetBufferFloat24Ch1(double[] left, double[] right) {
        mFs.Read(mBuffer, 0, mBuffer.Length);
        m4byte[0] = 0;
        for (int i = 0, j = 0; i < left.Length; i++, j += 3) {
            m4byte[1] = mBuffer[j];
            m4byte[2] = mBuffer[j + 1];
            m4byte[3] = mBuffer[j + 2];
            left[i] = BitConverter.ToSingle(m4byte, 0);
        }
    }
    void SetBufferFloat24Ch2(double[] left, double[] right) {
        mFs.Read(mBuffer, 0, mBuffer.Length);
        m4byte[0] = 0;
        for (int i = 0, j = 0; i < left.Length; i++, j += 6) {
            m4byte[1] = mBuffer[j];
            m4byte[2] = mBuffer[j + 1];
            m4byte[3] = mBuffer[j + 2];
            left[i] = BitConverter.ToSingle(m4byte, 0);
            m4byte[1] = mBuffer[j + 3];
            m4byte[2] = mBuffer[j + 4];
            m4byte[3] = mBuffer[j + 5];
            right[i] = BitConverter.ToSingle(m4byte, 0);
        }
    }
    void SetBufferFloat32Ch1(double[] left, double[] right) {
        mFs.Read(mBuffer, 0, mBuffer.Length);
        for (int i = 0, j = 0; i < left.Length; i++, j += 4) {
            left[i] = BitConverter.ToSingle(mBuffer, j);
        }
    }
    void SetBufferFloat32Ch2(double[] left, double[] right) {
        mFs.Read(mBuffer, 0, mBuffer.Length);
        for (int i = 0, j = 0; i < left.Length; i++, j += 8) {
            left[i] = BitConverter.ToSingle(mBuffer, j);
            right[i] = BitConverter.ToSingle(mBuffer, j + 4);
        }
    }
    void SetBufferFloat64Ch1(double[] left, double[] right) {
        mFs.Read(mBuffer, 0, mBuffer.Length);
        for (int i = 0, j = 0; i < left.Length; i++, j += 8) {
            left[i] = BitConverter.ToDouble(mBuffer, j);
        }
    }
    void SetBufferFloat64Ch2(double[] left, double[] right) {
        mFs.Read(mBuffer, 0, mBuffer.Length);
        for (int i = 0, j = 0; i < left.Length; i++, j += 16) {
            left[i] = BitConverter.ToDouble(mBuffer, j);
            right[i] = BitConverter.ToDouble(mBuffer, j + 8);
        }
    }

    public delegate void DWrite(short[] left, short[] right = null);
    public delegate void DDWrite(double[] left, double[] right = null);
    public DWrite WriteInt;
    public DDWrite WriteFloat;
    void WriteIntNop(short[] left, short[] right) { }
    void WriteInt8Ch1(short[] left, short[] right) {
        for (int i = 0; i < left.Length; i++) {
            mBuffer[i] = (byte)((left[i] >> 16) + 128);
        }
        mFs.Write(mBuffer, 0, mBuffer.Length);
        Samples += left.Length;
    }
    void WriteInt8Ch2(short[] left, short[] right) {
        for (int i = 0, j = 0; i < left.Length; i++, j += 2) {
            mBuffer[i] = (byte)((left[i] >> 16) + 128);
            mBuffer[i + 2] = (byte)((right[i] >> 16) + 128);
        }
        mFs.Write(mBuffer, 0, mBuffer.Length);
        Samples += left.Length;
    }
    void WriteInt16Ch1(short[] left, short[] right) {
        for (int i = 0, j = 0; i < left.Length; i++, j += 2) {
            Array.Copy(BitConverter.GetBytes(left[i]), 0, mBuffer, j, 2);
        }
        mFs.Write(mBuffer, 0, mBuffer.Length);
        Samples += left.Length;
    }
    void WriteInt16Ch2(short[] left, short[] right) {
        for (int i = 0, j = 0; i < left.Length; i++, j += 4) {
            Array.Copy(BitConverter.GetBytes(left[i]), 0, mBuffer, j, 2);
            Array.Copy(BitConverter.GetBytes(right[i]), 0, mBuffer, j + 2, 2);
        }
        mFs.Write(mBuffer, 0, mBuffer.Length);
        Samples += left.Length;
    }
    void WriteInt24Ch1(short[] left, short[] right) {
        for (int i = 0, j = 0; i < left.Length; i++, j += 3) {
            left[i] = BitConverter.ToInt16(mBuffer, j + 1);
        }
        mFs.Write(mBuffer, 0, mBuffer.Length);
        Samples += left.Length;
    }
    void WriteInt24Ch2(short[] left, short[] right) {
        for (int i = 0, j = 0; i < left.Length; i++, j += 6) {
            left[i] = BitConverter.ToInt16(mBuffer, j + 1);
            right[i] = BitConverter.ToInt16(mBuffer, j + 4);
        }
        mFs.Write(mBuffer, 0, mBuffer.Length);
        Samples += left.Length;
    }
    void WriteInt32Ch1(short[] left, short[] right) {
        for (int i = 0, j = 0; i < left.Length; i++, j += 4) {
            left[i] = BitConverter.ToInt16(mBuffer, j + 2);
        }
        mFs.Write(mBuffer, 0, mBuffer.Length);
        Samples += left.Length;
    }
    void WriteInt32Ch2(short[] left, short[] right) {
        for (int i = 0, j = 0; i < left.Length; i++, j += 8) {
            left[i] = BitConverter.ToInt16(mBuffer, j + 2);
            right[i] = BitConverter.ToInt16(mBuffer, j + 6);
        }
        mFs.Write(mBuffer, 0, mBuffer.Length);
        Samples += left.Length;
    }
    void WriteFloatNop(double[] left, double[] right) { }
    void WriteFloat24Ch1(double[] left, double[] right) {
        for (int i = 0, j = 0; i < left.Length; i++, j += 3) {
            Array.Copy(BitConverter.GetBytes((float)left[i]), 0, mBuffer, j, 3);
        }
        mFs.Write(mBuffer, 0, mBuffer.Length);
        Samples += left.Length;
    }
    void WriteFloat24Ch2(double[] left, double[] right) {
        for (int i = 0, j = 0; i < left.Length; i++, j += 6) {
            Array.Copy(BitConverter.GetBytes((float)left[i]), 0, mBuffer, j, 3);
            Array.Copy(BitConverter.GetBytes((float)right[i]), 0, mBuffer, j + 3, 3);
        }
        mFs.Write(mBuffer, 0, mBuffer.Length);
        Samples += left.Length;
    }
    void WriteFloat32Ch1(double[] left, double[] right) {
        for (int i = 0, j = 0; i < left.Length; i++, j += 4) {
            Array.Copy(BitConverter.GetBytes((float)left[i]), 0, mBuffer, j, 4);
        }
        mFs.Write(mBuffer, 0, mBuffer.Length);
        Samples += left.Length;
    }
    void WriteFloat32Ch2(double[] left, double[] right) {
        for (int i = 0, j = 0; i < left.Length; i++, j += 8) {
            Array.Copy(BitConverter.GetBytes((float)left[i]), 0, mBuffer, j, 4);
            Array.Copy(BitConverter.GetBytes((float)right[i]), 0, mBuffer, j + 4, 4);
        }
        mFs.Write(mBuffer, 0, mBuffer.Length);
        Samples += left.Length;
    }
    void WriteFloat64Ch1(double[] left, double[] right) {
        for (int i = 0, j = 0; i < left.Length; i++, j += 8) {
            Array.Copy(BitConverter.GetBytes(left[i]), 0, mBuffer, j, 8);
        }
        mFs.Write(mBuffer, 0, mBuffer.Length);
        Samples += left.Length;
    }
    void WriteFloat64Ch2(double[] left, double[] right) {
        for (int i = 0, j = 0; i < left.Length; i++, j += 16) {
            Array.Copy(BitConverter.GetBytes(left[i]), 0, mBuffer, j, 8);
            Array.Copy(BitConverter.GetBytes(right[i]), 0, mBuffer, j + 8, 8);
        }
        mFs.Write(mBuffer, 0, mBuffer.Length);
        Samples += left.Length;
    }
}