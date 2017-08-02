namespace NetCoreUv
{
    public class UvPrepareHandle : UvHandle
    {
        public void Init(UvLoopHandle loopHandle)
        {
            int prepareHandleSize = UvNative.uv_handle_size(UvNative.HandleType.PREPARE);

            InitUnmanaged(prepareHandleSize);

            UvNative.uv_prepare_init(loopHandle, this);
        }

        public void Start(UvNative.uv_prepare_cb prepareCallback)
        {
            UvNative.uv_prepare_start(this, prepareCallback);
        }

        public void Stop()
        {
            UvNative.uv_prepare_stop(this);
        }
    }
}