using System;

class ACF {
    public double[] Output { get; private set; }
    double mMin;
    double[] mInput;
    double[] mRe;
    double[] mIm;
    FFT mFFT;
    FFT mIFFT;

    public bool EliminateSlope = false;

    public ACF(int size, double minDb = -96) {
        size = (int)Math.Pow(2, Math.Ceiling(Math.Log(size, 2)));
        Output = new double[size / 2];
        mMin = Math.Pow(10, minDb / 20);
        mInput = new double[size];
        mRe = new double[size];
        mIm = new double[size];
        mFFT = new FFT(size);
        mIFFT = new FFT(size, true);
    }

    public void Exec(short[] input, bool update = false) {
        var N = mRe.Length;
        if (update) {
            Array.Copy(mInput, input.Length, mInput, 0, N - input.Length);
        }
        for (int i = 0, j = N - input.Length; i < input.Length; i++, j++) {
            mInput[j] = input[i] / 32768.0;
        }
        Array.Copy(mInput, 0, mRe, 0, N);
        Array.Clear(mIm, 0, mIm.Length);
        mFFT.Exec(mRe, mIm);
        for (int i = 0; i < N; i++) {
            var re = (mRe[i] * mRe[i]) + (mIm[i] * mIm[i]);
            mIm[i] = 0.0;
            mRe[i] = re;
        }
        mIFFT.Exec(mRe, mIm);
        double mBase;
        if (mRe[0] < mMin) {
            mBase = mMin;
        } else {
            mBase = mRe[0];
        }
        for (int i = 0; i < Output.Length; i++) {
            Output[i] = mRe[i] / mBase;
        }
    }
}
