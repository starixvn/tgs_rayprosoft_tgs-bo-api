namespace TGS.Tests.BackendOffice.UnitTests;

internal class StubWriteStream : Stream
{
    private long _length = 0L;

    public override bool CanRead { get; }

    public override bool CanSeek { get; }

    public override bool CanWrite => true;

    public override long Length => _length;

    public override long Position { get; set; }

    public override void Flush() { }

    public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

    public override void SetLength(long value) => throw new NotImplementedException();

    public override void Write(byte[] buffer, int offset, int count) => _length += count;
}
