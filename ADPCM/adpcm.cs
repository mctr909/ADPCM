using System.IO;

class ADPCM2 : RiffFile {
	public enum TYPE {
		BIT1 = 1,
		BIT2 = 2,
		BIT3 = 3,
		BIT4 = 4
	}

	public readonly int Samples;
	public readonly int PackBytes;

	readonly int BIT;
	readonly int UNIT_SAMPLES;
	readonly int UNIT_BYTES;

	readonly int[] MASK = { 0b1, 0b11, 0b111, 0b1111 };
	readonly int[] MAX_VALUE = { 0, 1, 3, 7 };
	readonly int[] MIN_VALUE = { 0, -2, -4, -8 };
	readonly double[] KE = { 0.6, 0.75, 0.75, 0.75 };
	readonly double[] KD = { 0.0, 3.00, 3.00, 3.00 };
	readonly double[][] DELTA_STEP = {
		new double[] {
			1.7500, // 0b000
			0.6250, // 0b001
			0.9325, // 0b010
			1.2500, // 0b011
			1.2500, // 0b100
			0.9325, // 0b101
			0.6250, // 0b110
			1.7500  // 0b111
		}, // 1bit
		new double[] { 0.6250, 1.5000, 1.5000 }, // 2bit
		new double[] { 0.7500, 1.2500, 1.2500, 1.7500, 1.7500 }, // 3bit
		new double[] { 0.7500, 1.1250, 1.1250, 1.1250, 1.1250, 1.5, 1.5, 1.85, 1.85 } // 4bit
	};

	int mType;
	int mCodeD;
	double mDelta = 4.0;
	double mPredict = 0.0;
	double mFilter = 0.0;

	long mPosFmt;
    long mPosData;

    public ADPCM2(int packes, TYPE type) {
		switch (type) {
		case TYPE.BIT1:
			mType = 0;
			mCodeD = 0;
			BIT = 1;
			UNIT_SAMPLES = 8;
			UNIT_BYTES = 1;
			break;
		case TYPE.BIT2:
			mType = 1;
			BIT = 2;
			UNIT_SAMPLES = 4;
			UNIT_BYTES = 1;
			break;
		case TYPE.BIT3:
			mType = 2;
			BIT = 3;
			UNIT_SAMPLES = 8;
			UNIT_BYTES = 3;
			break;
		case TYPE.BIT4:
			mType = 3;
			BIT = 4;
			UNIT_SAMPLES = 2;
			UNIT_BYTES = 1;
			break;
		}
		Samples = UNIT_SAMPLES * packes;
		PackBytes = UNIT_BYTES * packes;
	}

    public ADPCM2(string inputPath) : base(inputPath) {
        if (IsLoadComplete) {
            mFs.Position = mPosData;
        }
    }

    protected override bool CheckType(string type) {
        return "ADPM" == type;
    }

    protected override bool LoadChunk(string type, int size) {
        switch (type) {
        case "fmt ":
			mPosFmt = mFs.Position;
            break;
        case "data":
			mPosData = mFs.Position;
            break;
        }
        return false;
    }

    protected override bool LoadList(string type, int size) {
        return false;
    }

    public void DecodeFile(string outputPath) {
        if (!IsLoadComplete) {
            return;
        }

        var br = new BinaryReader(mFs);

        mFs.Position = mPosFmt;
        var tag = br.ReadUInt16();
        var channels = br.ReadUInt16();
        var sampleRate = br.ReadInt32();
        var bytesPerSec = br.ReadUInt32();
        var packes = br.ReadUInt16();
        var type = (TYPE)br.ReadUInt16();

        mFs.Position = mPosData;
        var wav = new RiffWave(
            outputPath,
            2 == channels ? RiffWave.TYPE.INT16_CH2 : RiffWave.TYPE.INT16_CH1,
            sampleRate
        );
        switch (wav.Channels) {
        case 1: {
            var adpcm = new ADPCM2(packes, type);
            var output = new short[adpcm.Samples];
            var input = new byte[adpcm.PackBytes];
            wav.AllocateBuffer(adpcm.Samples);
            while (mFs.Position < mFs.Length) {
                mFs.Read(input, 0, adpcm.PackBytes);
                adpcm.Decode(output, input);
                wav.WriteInt(output);
            }
            break;
        }
        case 2: {
            var adpcmL = new ADPCM2(packes, type);
            var adpcmR = new ADPCM2(packes, type);
            var outputL = new short[adpcmL.Samples];
            var outputR = new short[adpcmR.Samples];
            var inputL = new byte[adpcmL.PackBytes];
            var inputR = new byte[adpcmR.PackBytes];
            wav.AllocateBuffer(adpcmL.Samples);
            while (mFs.Position < mFs.Length) {
                mFs.Read(inputL, 0, adpcmL.PackBytes);
                mFs.Read(inputR, 0, adpcmR.PackBytes);
                adpcmL.Decode(outputL, inputL);
                adpcmR.Decode(outputR, inputR);
                wav.WriteInt(outputL, outputR);
            }
            break;
        }
        }
        wav.Close();
    }

    public static bool EncodeFile(string inputPath, string outputPath, TYPE type, int packes) {
        var wav = new RiffWave(inputPath);
        if (!wav.IsLoadComplete) {
            wav.Close();
            return false;
        }

        var fs = new FileStream(outputPath, FileMode.Create);
        var bw = new BinaryWriter(fs);
        bw.Write(new byte[44]);

        switch (wav.Channels) {
        case 1: {
            var adpcm = new ADPCM2(packes, type);
            var input = new short[adpcm.Samples];
            var output = new byte[adpcm.PackBytes];
            wav.AllocateBuffer(adpcm.Samples);
            for (int i = 0; i < wav.Samples; i += adpcm.Samples) {
                wav.SetBufferInt(input);
                adpcm.Encode(input, output);
                fs.Write(output, 0, adpcm.PackBytes);
            }
            break;
        }
        case 2: {
            var adpcmL = new ADPCM2(packes, type);
            var adpcmR = new ADPCM2(packes, type);
            var inputL = new short[adpcmL.Samples];
            var inputR = new short[adpcmR.Samples];
            var outputL = new byte[adpcmL.PackBytes];
            var outputR = new byte[adpcmR.PackBytes];
            wav.AllocateBuffer(adpcmL.Samples);
            for (int i = 0; i < wav.Samples; i += adpcmL.Samples) {
                wav.SetBufferInt(inputL, inputR);
                adpcmL.Encode(inputL, outputL);
                adpcmR.Encode(inputR, outputR);
                fs.Write(outputL, 0, adpcmL.PackBytes);
                fs.Write(outputR, 0, adpcmR.PackBytes);
            }
            break;
        }
        }

        fs.Position = 0;
        bw.Write(new char[] { 'R', 'I', 'F', 'F' });
        bw.Write((uint)(fs.Length - 8));
        bw.Write(new char[] { 'A', 'D', 'P', 'M' });

        bw.Write(new char[] { 'f', 'm', 't', ' ' });
        bw.Write((uint)16);
        bw.Write((ushort)0);
        bw.Write((ushort)wav.Channels);
        bw.Write(wav.SampleRate);
        bw.Write((uint)(wav.SampleRate * wav.Channels * (int)type >> 3));
        bw.Write((ushort)packes);
        bw.Write((ushort)type);

        bw.Write(new char[] { 'd', 'a', 't', 'a' });
        bw.Write((uint)(fs.Length - 44));

        fs.Close();
        fs.Dispose();
        wav.Close();
        return true;
    }

    public void Encode(short[] p_input, byte[] p_output) {
        for (int si = 0, bi = 0; si < Samples; si += UNIT_SAMPLES, bi += UNIT_BYTES) {
            long output = 0;
            for (int j = 0, sj = si; j < UNIT_SAMPLES && sj < Samples; j++, sj++) {
                /*** フィルター ***/
                mFilter = mFilter * KE[mType] + p_input[sj] * (1.0 - KE[mType]);
                /*** エンコード ***/
                int code;
                if (0 == mType) {
                    code = (0 <= (mFilter - mPredict) / mDelta) ? 1 : 0;
                    update1bit(code);
                } else {
                    code = (int)((mFilter - mPredict) / mDelta);
                    if (code < MIN_VALUE[mType]) {
                        code = MIN_VALUE[mType];
                    }
                    if (MAX_VALUE[mType] < code) {
                        code = MAX_VALUE[mType];
                    }
                    update(code);
                }
                if (code < 0) {
                    output |= (long)(code + MASK[mType] + 1) << (BIT * j);
                } else {
                    output |= (long)code << (BIT * j);
                }
            }
            for (int j = 0, bj = bi; j < UNIT_BYTES; j++, bj++) {
                p_output[bj] = (byte)((output >> (8 * j)) & 0xFF);
            }
        }
    }
    
    public void Decode(short[] p_output, byte[] p_input) {
        for (int si = 0, bi = 0; si < Samples; si += UNIT_SAMPLES, bi += UNIT_BYTES) {
            long input = 0;
            for (int j = 0, bj = bi; j < UNIT_BYTES; j++, bj++) {
                input |= (long)p_input[bj] << (8 * j);
            }
            for (int j = 0, sj = si; j < UNIT_SAMPLES && sj < Samples; j++, sj++) {
                /*** デコード ***/
                var code = (int)(input >> (BIT * j)) & MASK[mType];
                double output;
                if (0 == mType) {
                    update1bit(code);
                } else {
                    if (MAX_VALUE[mType] < code) {
                        code -= MASK[mType] + 1;
                    }
                    update(code);
                }
                /*** 出力 ***/
                output = mPredict + (mPredict - mFilter) * KD[mType];
                mFilter = mPredict;
                if (output < -32768) {
                    output = -32768;
                }
                if (32767 < output) {
                    output = 32767;
                }
                p_output[sj] = (short)output;
            }
        }
    }

    void update(int code) {
		if (code < 0) {
			mDelta *= DELTA_STEP[mType][-code];
		} else {
			mDelta *= DELTA_STEP[mType][code];
		}
		if (mDelta < 3) {
			mDelta = 3;
		}
		if (16384 < mDelta) {
			mDelta = 16384;
		}
		mPredict += code * mDelta;
	}
	
    void update1bit(int code) {
		mCodeD = (mCodeD << 1) & 0b111;
		mCodeD |= code;
		mDelta *= DELTA_STEP[0][mCodeD];
		if (mDelta < 0.25) {
			mDelta = 0.25;
		}
		if (16384 < mDelta) {
			mDelta = 16384;
		}
		if (1 == code) {
			mPredict += mDelta;
		} else {
			mPredict -= mDelta;
		}
	}
}