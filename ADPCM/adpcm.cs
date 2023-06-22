using System.IO;

class ADPCM2 {
	public enum TYPE {
		BIT1 = 1,
		BIT2 = 2,
		BIT3 = 3,
		BIT4 = 4
	}

	public readonly int Bit;
	public readonly int PackingSamples;
	public readonly int PackingBytes;

	public int Samples { get; private set; }

	readonly int[] MASK = { 0b1, 0b11, 0b111, 0b1111 };
	readonly int[] MAX_VALUE = { 0, 1, 3, 7 };
	readonly int[] MIN_VALUE = { 0, -2, -4, -8 };
	readonly double[][] DELTA_STEP = {
		new double[] {
			1.7500, // 0b000
			0.5000, // 0b001
			0.9375, // 0b010
			1.3750, // 0b011
			1.3750, // 0b100
			0.9375, // 0b101
			0.5000, // 0b110
			1.7500  // 0b111
		}, // 1bit
        new double[] { 0.75, 1.375, 1.375 }, // 2bit
		new double[] { 0.75, 1.0, 1.25, 1.75, 1.75 }, // 3bit
		new double[] { 0.75, 1.0, 1.0, 1.125, 1.125, 1.5, 1.5, 1.75, 1.75 } // 4bit
	};

	int mType;
	int mCodeD;
	double mDelta = 4.0;
	double mPredict = 0.0;
	double mFilter = 0.0;

	public ADPCM2(int packes, TYPE type) {
		switch (type) {
		case TYPE.BIT1:
			mType = 0;
			mCodeD = 0;
			Bit = 1;
			PackingSamples = 32;
			PackingBytes = 4;
			break;
		case TYPE.BIT2:
			mType = 1;
			Bit = 2;
			PackingSamples = 24;
			PackingBytes = 6;
			break;
		case TYPE.BIT3:
			mType = 2;
			Bit = 3;
			PackingSamples = 16;
			PackingBytes = 6;
			break;
		case TYPE.BIT4:
			mType = 3;
			Bit = 4;
			PackingSamples = 12;
			PackingBytes = 6;
			break;
		}
		Samples = PackingSamples * packes;
	}

	void update(int code) {
		if (code < 0) {
			mDelta *= DELTA_STEP[mType][-code];
		} else {
			mDelta *= DELTA_STEP[mType][code];
		}
		if (mDelta < 1) {
			mDelta = 1;
		}
		if (12288 < mDelta) {
			mDelta = 12288;
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

	public void Encode(short[] p_input, byte[] p_output) {
		for (int si = 0, bi = 0; si < Samples; si += PackingSamples, bi += PackingBytes) {
			long output = 0;
			for (int j = 0, sj = si; j < PackingSamples && sj < Samples; j++, sj++) {
				/*** フィルタ ***/
				mFilter = (mFilter + p_input[sj]) * 0.5;
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
					output |= (long)(code + MASK[mType] + 1) << (Bit * j);
				} else {
					output |= (long)code << (Bit * j);
				}
			}
			for (int j = 0, bj = bi; j < PackingBytes; j++, bj++) {
				p_output[bj] = (byte)((output >> (8 * j)) & 0xFF);
			}
		}
	}
	public void Decode(short[] p_output, byte[] p_input) {
		for (int si = 0, bi = 0; si < Samples; si += PackingSamples, bi += PackingBytes) {
			long input = 0;
			for (int j = 0, bj = bi; j < PackingBytes; j++, bj++) {
				input |= (long)p_input[bj] << (8 * j);
			}
			for (int j = 0, sj = si; j < PackingSamples && sj < Samples; j++, sj++) {
				/*** デコード ***/
				var code = (int)(input >> (Bit * j)) & MASK[mType];
				if (0 == mType) {
					update1bit(code);
				} else {
					if (MAX_VALUE[mType] < code) {
						code -= MASK[mType] + 1;
					}
					update(code);
				}
				/*** 出力 ***/
				var output = mPredict + (mPredict - mFilter);
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

	public static bool EncodeFile(string inputPath, string outputPath, TYPE type) {
		var wav = new RiffWave(inputPath);
		if (!wav.IsLoadComplete) {
			wav.Close();
			return false;
		}

		var fs = new FileStream(outputPath, FileMode.Create);
		var bw = new BinaryWriter(fs);
		bw.Write(wav.SampleRate);
		bw.Write((ushort)wav.Channels);
		bw.Write((ushort)type);

		switch (wav.Channels) {
		case 1: {
			var adpcm = new ADPCM2(1, type);
			var input = new short[adpcm.PackingSamples];
			var output = new byte[adpcm.PackingBytes];
			wav.AllocateBuffer(adpcm.PackingSamples);
			for (int i = 0; i < wav.Samples; i += adpcm.PackingSamples) {
				wav.SetBuffer(input);
				adpcm.Encode(input, output);
				fs.Write(output, 0, adpcm.PackingBytes);
			}
			break;
		}
		case 2: {
			var adpcmL = new ADPCM2(1, type);
			var adpcmR = new ADPCM2(1, type);
			var inputL = new short[adpcmL.PackingSamples];
			var inputR = new short[adpcmR.PackingSamples];
			var outputL = new byte[adpcmL.PackingBytes];
			var outputR = new byte[adpcmR.PackingBytes];
			wav.AllocateBuffer(adpcmL.PackingSamples);
			for (int i = 0; i < wav.Samples; i += adpcmL.PackingSamples) {
				wav.SetBuffer(inputL, inputR);
				adpcmL.Encode(inputL, outputL);
				adpcmR.Encode(inputR, outputR);
				fs.Write(outputL, 0, adpcmL.PackingBytes);
				fs.Write(outputR, 0, adpcmR.PackingBytes);
			}
			break;
		}
		}
		fs.Close();
		fs.Dispose();
		wav.Close();
		return true;
	}
	public static void DecodeFile(string inputPath, string outputPath) {
		var fs = new FileStream(inputPath, FileMode.Open);
		var br = new BinaryReader(fs);
		var sampleRate = br.ReadInt32();
		var channels = br.ReadUInt16();
		var type = (TYPE)br.ReadUInt16();
		var wav = new RiffWave(
			outputPath,
			2==channels ? RiffWave.TYPE.INT16_CH2 : RiffWave.TYPE.INT16_CH1,
			sampleRate
		);

		switch (wav.Channels) {
		case 1: {
			var adpcm = new ADPCM2(1, type);
			var output = new short[adpcm.PackingSamples];
			var input = new byte[adpcm.PackingBytes];
			wav.AllocateBuffer(adpcm.PackingSamples);
			while (fs.Position < fs.Length) {
				fs.Read(input, 0, adpcm.PackingBytes);
				adpcm.Decode(output, input);
				wav.Write(output);
			}
			break;
		}
		case 2: {
			var adpcmL = new ADPCM2(1, type);
			var adpcmR = new ADPCM2(1, type);
			var outputL = new short[adpcmL.PackingSamples];
			var outputR = new short[adpcmR.PackingSamples];
			var inputL = new byte[adpcmL.PackingBytes];
			var inputR = new byte[adpcmR.PackingBytes];
			wav.AllocateBuffer(adpcmL.PackingSamples);
			while (fs.Position < fs.Length) {
				fs.Read(inputL, 0, adpcmL.PackingBytes);
				fs.Read(inputR, 0, adpcmR.PackingBytes);
				adpcmL.Decode(outputL, inputL);
				adpcmR.Decode(outputR, inputR);
				wav.Write(outputL, outputR);
			}
			break;
		}
		}
		fs.Close();
		fs.Dispose();
		wav.Close();
	}
}