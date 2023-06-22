using System.IO;
using System.Text;

abstract class RiffFile {
    public bool IsLoadComplete { get; protected set; } = false;
    public long Length {
        get { return mFs.Length; }
    }
    public long Position {
        get { return mFs.Position; }
        set { mFs.Position = value; }
    }

    protected int mFileSize = 0;
    protected FileStream mFs = null;

    public RiffFile() { }
    public RiffFile(string path) {
        load(path);
    }

    public virtual void Close() {
        if (null != mFs) {
            mFs.Close();
            mFs.Dispose();
            mFs = null;
        }
    }

    void load(string path) {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) {
            return;
        }

        mFs = new FileStream(path, FileMode.Open, FileAccess.Read);
        if (mFs.Length < 12) {
            Close();
            return;
        }

        var bRiffId = new byte[4];
        var bFileSize = new byte[4];
        var bFileType = new byte[4];
        mFs.Read(bRiffId, 0, bRiffId.Length);
        mFs.Read(bFileSize, 0, bFileSize.Length);
        mFs.Read(bFileType, 0, bFileType.Length);

        if ("RIFF" != Encoding.ASCII.GetString(bRiffId)) {
            Close();
            return;
        }
        if (!CheckType(Encoding.ASCII.GetString(bFileType))) {
            Close();
            return;
        }

        mFileSize = bFileSize[0];
        mFileSize |= bFileSize[1] << 8;
        mFileSize |= bFileSize[2] << 16;
        mFileSize |= bFileSize[3] << 24;
        mFileSize += 8;
        if (mFs.Length < mFileSize) {
            mFileSize = 0;
            Close();
            return;
        }

        while (mFs.Position < mFileSize) {
            var bChunkType = new byte[4];
            var bChunkSize = new byte[4];
            mFs.Read(bChunkType, 0, bChunkType.Length);
            mFs.Read(bChunkSize, 0, bChunkSize.Length);
            var chunkType = Encoding.ASCII.GetString(bChunkType);
            var chunkSize = (int)bChunkSize[0];
            chunkSize |= bChunkSize[1] << 8;
            chunkSize |= bChunkSize[2] << 16;
            chunkSize |= bChunkSize[3] << 24;
            if ("LIST" == chunkType) {
                var bListType = new byte[4];
                mFs.Read(bListType, 0, bListType.Length);
                var listType = Encoding.ASCII.GetString(bListType);
                var listSize = chunkSize - 4;
                if (!LoadList(listType, listSize)) {
                    mFs.Seek(listSize, SeekOrigin.Current);
                }
            } else {
                if (!LoadChunk(chunkType, chunkSize)) {
                    mFs.Seek(chunkSize, SeekOrigin.Current);
                }
            }
        }

        LoadComplete();
    }

    protected abstract bool CheckType(string type);
    protected abstract bool LoadChunk(string type, int size);
    protected abstract bool LoadList(string type, int size);

    protected virtual void LoadComplete() {
        IsLoadComplete = true;
    }
}
