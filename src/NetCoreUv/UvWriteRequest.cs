using System;

namespace NetCoreUv
{
    public class UvWriteRequest : UvHandle
    {
        [ThreadStatic]
        static private UvNative.uv_buf_t[] _writeBufs;

        protected void Init()
        {
            int writeRequestSize = UvNative.uv_req_size(UvNative.RequestType.WRITE);
            
            InitUnmanaged(writeRequestSize);
        }

        protected int Write(UvTcpHandle tcpHandle, UvNative.uv_buf_t buf)
        {
            _writeBufs = _writeBufs ?? new UvNative.uv_buf_t[1];
            _writeBufs[0] = buf;
            
            return UvNative.uv_write(
                this,
                tcpHandle,
                _writeBufs,
                1,
                WriteCallback
            );
        }

        protected virtual void OnWrited()
        {
        }
        
        private void WriteCallback(IntPtr writeRequestPtr, int status)
        {
            OnWrited();
        }
    }
}