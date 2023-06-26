class ADPCM2 {
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
	readonly double[] KE = { 0.33, 0.25, 0.25, 0.00 };
	readonly double[] KD = { 0.00, 0.25, 0.33, 0.00 };
	readonly double[][] DELTA_STEP = {
		new double[] {
			17/16.0, // 0b00
			15/16.0, // 0b01
			15/16.0, // 0b10
			17/16.0, // 0b11
		}, // 1bit
		new double[] { 0.9375, 1.1250, 1.1250 }, // 2bit
		new double[] { 0.7500, 1.1250, 1.1250, 1.7500, 1.7500 }, // 3bit
		new double[] { 0.7500, 1.1250, 1.1250, 1.1250, 1.1250, 1.5, 1.5, 1.85, 1.85 } // 4bit
	};

	int mType;
	int mCodeD;
	double mDelta = 0.25;
	double mPredict = 0.0;
	double mFilter = 0.0;

	public ADPCM2(TYPE type, int packes = 1) {
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

	public void Clear() {
		mDelta = 1024.0;
		mPredict = 0.0;
		mFilter = 0.0;
	}

	public void Encode(short[] p_input, byte[] p_output) {
		for (int si = 0, bi = 0; si < Samples; si += UNIT_SAMPLES, bi += UNIT_BYTES) {
			long output = 0;
			for (int j = 0, sj = si; j < UNIT_SAMPLES && sj < Samples; j++, sj++) {
				/*** エンコード ***/
				mFilter = mFilter * KE[mType] + p_input[sj] * (1.0 - KE[mType]);
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

	public void Decode(short[] p_output, byte[] p_input, int offset = 0) {
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
					/*** 出力 ***/
					output = (mPredict + mFilter) * 0.5;
					mFilter = mPredict;
				} else {
					if (MAX_VALUE[mType] < code) {
						code -= MASK[mType] + 1;
					}
					update(code);
					/*** 出力 ***/
					output = mPredict + (mPredict - mFilter) * KD[mType];
				}
				mFilter = mPredict;
				if (output < -32768) {
					output = -32768;
				}
				if (32767 < output) {
					output = 32767;
				}
				p_output[sj + offset] = (short)output;
			}
		}
	}

	void update(int code) {
		if (code < 0) {
			mDelta *= DELTA_STEP[mType][-code];
		} else {
			mDelta *= DELTA_STEP[mType][code];
		}
		if (mDelta < 0.25) {
			mDelta = 0.25;
		}
		if (16384 < mDelta) {
			mDelta = 16384;
		}
		mPredict += code * mDelta;
		if (32767 < mPredict) {
			mPredict = 32767;
		}
		if (mPredict < -32768) {
			mPredict = -32768;
		}
	}

	void update1bit(int code) {
		mCodeD = (mCodeD << 1) & 0b11;
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
		if (32767 < mPredict) {
			mPredict = 32767;
		}
		if (mPredict < -32768) {
			mPredict = -32768;
		}
	}
}