namespace NetCoreUv
{
    public class UvConnectRequest : UvHandle
    {
        public void Init()
        {
            int connectRequestSize = UvNative.uv_req_size(UvNative.RequestType.CONNECT);
            
            InitUnmanaged(connectRequestSize);
        }
    }
}