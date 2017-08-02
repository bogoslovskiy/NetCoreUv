using System;

namespace NetCoreUv
{
    public class UvLoopHandle : UvHandleBase
    {
        public void Init()
        {
            int loopSize = UvNative.uv_loop_size();
            InitUnmanaged(loopSize);
            UvNative.uv_loop_init(this);
        }

        public int RunDefault()
        {
            return UvNative.uv_run(this, 0 /* UV_RUN_DEFAULT */);
        }
        
        public int RunOnce()
        {
            return UvNative.uv_run(this, 1 /* UV_RUN_ONCE */);
        }

        public void Stop()
        {
            UvNative.uv_stop(this);
        }

        unsafe protected override bool ReleaseHandle()
        {
            IntPtr handlePtr = handle;
            if (handlePtr != IntPtr.Zero)
            {
                // uv_loop_close очищает gcHandlePtr.
                var gcHandlePtr = *(IntPtr*)handlePtr;

                UvNative.uv_loop_close(this.InternalGetHandle());

                handle = IntPtr.Zero;

                ReleaseUnmanaged(handlePtr, gcHandlePtr);
            }

            return true;
        }
    }
}