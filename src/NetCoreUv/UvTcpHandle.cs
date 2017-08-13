using System;
using System.Net;

namespace NetCoreUv
{
    public class UvTcpHandle : UvStreamHandle
    {
        private UvConnectRequest _connectRequest;
        private ConnectionCallback _connectionCallback;
        
        public void Init(UvLoopHandle loopHandle)
        {
            int tcpHandleSize = UvNative.uv_handle_size(UvNative.HandleType.TCP);

            InitUnmanaged(tcpHandleSize);

            UvNative.uv_tcp_init(loopHandle, this);
        }

        public void KeepAlive(uint delay)
        {
            UvNative.uv_tcp_keepalive(this, 1, delay);
        }
        
        public void Connect(ServerAddress address, ConnectionCallback connectionCallback)
        {
            SockAddr addr = GetSockAddr(address);
            
            _connectionCallback = connectionCallback;
            _connectRequest = new UvConnectRequest();
            _connectRequest.Init();
            UvNative.uv_tcp_connect(_connectRequest, this, ref addr, ConnectionCb);
        }

        public void Bind(ServerAddress address)
        {
            SockAddr addr = GetSockAddr(address);

            UvNative.uv_tcp_bind(this, ref addr, 0 /* flags */);
        }

        static private SockAddr GetSockAddr(ServerAddress address)
        {
            IPEndPoint endpoint = CreateIPEndpoint(address);

            var addressText = endpoint.Address.ToString();

            SockAddr addr;
            int ip4Status = UvNative.uv_ip4_addr(addressText, endpoint.Port, out addr);

            if (ip4Status < 0)
            {
                int ip6Status = UvNative.uv_ip6_addr(addressText, endpoint.Port, out addr);
                if (ip6Status < 0)
                {
                    // TODO:
                    throw new Exception();
                }
            }
            return addr;
        }

        private void ConnectionCb(IntPtr handle, int status)
        {
            // TODO: dispose _connectRequest?
            //UvTcpHandle tcpHandle = FromIntPtr<UvTcpHandle>(handle);
            _connectionCallback(this, status);
        }
        
        static private IPEndPoint CreateIPEndpoint(ServerAddress address)
        {
            // TODO: IPv6 support
            IPAddress ip;

            if (!IPAddress.TryParse(address.Host, out ip))
            {
                if (string.Equals(address.Host, "localhost", StringComparison.OrdinalIgnoreCase))
                {
                    ip = IPAddress.Loopback;
                }
                else
                {
                    ip = IPAddress.IPv6Any;
                }
            }

            return new IPEndPoint(ip, address.Port);
        }
    }
}