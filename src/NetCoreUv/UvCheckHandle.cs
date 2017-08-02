namespace NetCoreUv
{
    public class UvCheckHandle : UvHandle
    {
        public void Init(UvLoopHandle loopHandle)
        {
            int checkHandleSize = UvNative.uv_handle_size(UvNative.HandleType.CHECK);

            InitUnmanaged(checkHandleSize);

            UvNative.uv_check_init(loopHandle, this);
        }
    }
}