using System;

class VAG {
    public enum Flag {
        NOTHING = 0,
        LOOP_LAST_BLOCK = 1,
        LOOP_REGION = 2,
        LOOP_END = 3,
        LOOP_FIRST_BLOCK = 4,
        UNK = 5,
        LOOP_START = 6,
        PLAYBACK_END = 7
    };
    public const int PACKING_SAMPLES = 28;
    public byte[] EncBuf = new byte[PACKING_SAMPLES / 2 + 2];
    public short[] DecBuf = new short[PACKING_SAMPLES];

    static readonly double[,] ENC_K = new double[,] {
        { 0.0, 0.0 },
        {  -60.0 / 64.0, 0.0 },
        { -115.0 / 64.0, 52.0 / 64.0 },
        {  -98.0 / 64.0, 55.0 / 64.0 },
        { -122.0 / 64.0, 60.0 / 64.0 }
    };
    static readonly double[,] DEC_K = new double[,] {
        { 0.0, 0.0 },
        {  60.0 / 64.0, 0.0 },
        { 115.0 / 64.0, -52.0 / 64.0 },
        {  98.0 / 64.0, -55.0 / 64.0 },
        { 122.0 / 64.0, -60.0 / 64.0 }
    };

    double mF1 = 0.0;
    double mF2 = 0.0;
    double mS1 = 0.0;
    double mS2 = 0.0;
    double[,] mPredictBuf = new double[PACKING_SAMPLES, 5];

    public void Enc(short[] pcmData) {
        int predict = 0;
        double min = 1e10;
        double f1 = 0.0;
        double f2 = 0.0;
        for (int f = 0; f < 5; f++) {
            double max = 0.0;
            f1 = mF1;
            f2 = mF2;
            for (int i = 0; i < PACKING_SAMPLES; i++) {
                var sample = pcmData[i];
                if (sample > 30719) {
                    sample = 30719;
                }
                if (sample < -30720) {
                    sample = -30720;
                }
                var ds = sample + f1 * ENC_K[f, 0] + f2 * ENC_K[f, 1];
                f2 = f1;
                f1 = sample;
                mPredictBuf[i, f] = ds;
                if (Math.Abs(ds) > max) {
                    max = Math.Abs(ds);
                }
            }
            if (max < min) {
                min = max;
                predict = f;
            }
            if (min <= 7) {
                predict = 0;
                break;
            }
        }
        mF1 = f1;
        mF2 = f2;

        int min2 = (int)min;
        int shift_mask = 0x4000;
        int shift = 0;
        while (shift < 12) {
            if (Convert.ToBoolean(shift_mask & (min2 + (shift_mask >> 3)))) {
                break;
            }
            shift++;
            shift_mask >>= 1;
        }

        for (int i = 0, ib = 2; i < PACKING_SAMPLES; i += 2, ib++) {
            var d_trans = mPredictBuf[i, predict] + mS1 * ENC_K[predict, 0] + mS2 * ENC_K[predict, 1];
            var d_sample = d_trans * (1 << shift);
            var sample = (int)(((int)d_sample + 0x800) & 0xFFFFF000);
            if (sample > short.MaxValue) {
                sample = short.MaxValue;
            }
            if (sample < short.MinValue) {
                sample = short.MinValue;
            }
            mS2 = mS1;
            mS1 = (sample >> shift) - d_trans;
            var out1 = sample;

            d_trans = mPredictBuf[i + 1, predict] + mS1 * ENC_K[predict, 0] + mS2 * ENC_K[predict, 1];
            d_sample = d_trans * (1 << shift);
            sample = (int)(((int)d_sample + 0x800) & 0xFFFFF000);
            if (sample > short.MaxValue) {
                sample = short.MaxValue;
            }
            if (sample < short.MinValue) {
                sample = short.MinValue;
            }
            mS2 = mS1;
            mS1 = (sample >> shift) - d_trans;
            var out2 = sample;

            EncBuf[ib] = (byte)(((out1 >> 12) & 0x0F) | ((out2 >> 8) & 0xF0));
        }
        EncBuf[0] = (byte)(((predict << 4) & 0xF0) | (shift & 0x0F));
    }

    public void Dec(byte[] vagData) {
        var shift = vagData[0] & 0xF;
        var predict = (vagData[0] & 0xF0) >> 4;
        for (int i = 0, b = 2; i < PACKING_SAMPLES; i += 2, b++) {
            var in1 = (vagData[b] & 0x0F) << 12;
            if ((in1 & 0x8000) != 0) {
                in1 = (int)(in1 | 0xFFFF0000);
            }
            var out1 = (in1 >> shift) + mS1 * DEC_K[predict, 0] + mS2 * DEC_K[predict, 1];
            mS2 = mS1;
            mS1 = out1;

            var in2 = (vagData[b] & 0xF0) << 8;
            if ((in2 & 0x8000) != 0) {
                in2 = (int)(in2 | 0xFFFF0000);
            }
            var out2 = (in2 >> shift) + mS1 * DEC_K[predict, 0] + mS2 * DEC_K[predict, 1];
            mS2 = mS1;
            mS1 = out2;

            DecBuf[i] = (short)out1;
            DecBuf[i + 1] = (short)out2;
        }
    }
}
