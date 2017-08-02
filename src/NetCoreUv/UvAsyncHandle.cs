using System.Runtime.CompilerServices;

namespace NetCoreUv
{
    public class UvAsyncHandle : UvHandle
    {
        public void Init(UvLoopHandle loopHandle, UvNative.uv_async_cb asyncCallback)
        {
            int handleSize = UvNative.uv_handle_size(UvNative.HandleType.ASYNC);

            InitUnmanaged(handleSize);

            UvNative.uv_async_init(loopHandle, this, asyncCallback);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send()
        {
            UvNative.uv_async_send(this);
        }
    }
}