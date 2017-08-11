using System;

namespace NetCoreUv
{
    public class UvWriteRequest : UvHandle
    {
        [ThreadStatic]
        static private UvNative.uv_buf_t[] _writeBufs;

        private UvNative.uv_write_cb _writeCallback;
        
        public void Init(UvNative.uv_write_cb writeCallback)
        {
            _writeCallback = writeCallback;
            
            int writeRequestSize = UvNative.uv_req_size(UvNative.RequestType.WRITE);
            
            InitUnmanaged(writeRequestSize);
        }

        public int Write(UvTcpHandle tcpHandle, UvNative.uv_buf_t buf)
        {
            _writeBufs = _writeBufs ?? new UvNative.uv_buf_t[1];
            _writeBufs[0] = buf;

            return UvNative.uv_write(
                this,
                tcpHandle,
                _writeBufs,
                1,
                _writeCallback
            );
        }
    }
}