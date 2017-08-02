using System;

namespace NetCoreUv
{
    abstract public class UvHandle : UvHandleBase
    {
        public void Reference()
        {
            UvNative.uv_ref(this);
        }

        public void Unreference()
        {
            UvNative.uv_unref(this);
        }

        // TODO: проверка на поток
        protected override bool ReleaseHandle()
        {
            IntPtr handlePtr = handle;
            if (handlePtr != IntPtr.Zero)
            {
                handle = IntPtr.Zero;

                UvNative.uv_close(handlePtr, ReleaseUnmanaged);
            }
            return true;
        }
    }
}