
namespace ADPCM {
    class lowbit {
		int mHist;
		double mDelta;

        void delta2bit() {
			var output = 0;

			switch (mHist) {
			case 0b0000:
			case 0b1111:
				mDelta *= 2.3;
				break;
			case 0b0111:
			case 0b1000:
				mDelta *= 1.75;
				break;
			case 0b0011:
			case 0b0100:
			case 0b1011:
			case 0b1100:
				mDelta *= 1.2;
				break;
			case 0b0001:
			case 0b1110:
				mDelta *= 1;
				break;
			case 0b0010:
			case 0b1101:
			case 0b0110:
			case 0b1001:
				mDelta *= 0.75;
				break;
			case 0b0101:
			case 0b1010:
				mDelta *= 0.5;
				break;
			}
			mHist = (mHist & 0b1111) << 1;
			mHist |= output;
		}
	}
}
