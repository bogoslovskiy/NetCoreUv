using System;

namespace NetCoreUv
{
    abstract public class UvStreamHandle : UvHandle
    {
        public delegate void ConnectionCallback(UvStreamHandle streamHandle, int status);

        public delegate void AllocCallback(
            UvStreamHandle streamHandle,
            int suggestedSize,
            out UvNative.uv_buf_t buf);

        public delegate void ReadCallback(UvStreamHandle streamHandle, int status, ref UvNative.uv_buf_t buf);

        [ThreadStatic]
        static private UvNative.uv_buf_t[] _writeBufs;

        private ConnectionCallback _connectionCallback;
        private AllocCallback _allocCallback;
        private ReadCallback _readCallback;
        
        public void Listen(int backlog, ConnectionCallback connectionCallback)
        {
            _connectionCallback = connectionCallback;
            UvNative.uv_listen(this, backlog, ConnectionCb);
        }

        public void Accept(UvStreamHandle clientStreamHandle)
        {
            UvNative.uv_accept(this /* serverStreamHandle */, clientStreamHandle);
        }

        public void ReadStart(AllocCallback allocCallback, ReadCallback readCallback)
        {
            _allocCallback = allocCallback;
            _readCallback = readCallback;
            UvNative.uv_read_start(this, AllocCb, ReadCb);
        }

        public void ReadStop()
        {
            UvNative.uv_read_stop(this);
        }

        public int TryWrite(UvNative.uv_buf_t buf)
        {
            _writeBufs = _writeBufs ?? new UvNative.uv_buf_t[1];
            _writeBufs[0] = buf;

            return UvNative.uv_try_write(this, _writeBufs, 1);
        }

        static private void ConnectionCb(IntPtr handle, int status)
        {
            UvStreamHandle streamHandle = FromIntPtr<UvStreamHandle>(handle);
            streamHandle._connectionCallback(streamHandle, status);
        }

        static private void AllocCb(IntPtr handle, int suggestedSize, out UvNative.uv_buf_t buf)
        {
            UvStreamHandle streamHandle = FromIntPtr<UvStreamHandle>(handle);
            streamHandle._allocCallback(streamHandle, suggestedSize, out buf);
        }

        static private void ReadCb(IntPtr handle, int status, ref UvNative.uv_buf_t buf)
        {
            UvStreamHandle streamHandle = FromIntPtr<UvStreamHandle>(handle);
            streamHandle._readCallback(streamHandle, status, ref buf);
        }
    }
}