﻿using System;
using System.Diagnostics;
using System.Globalization;

namespace NetCoreUv
{
    public class ServerAddress
    {
        public string Host { get; private set; }
        public string PathBase { get; private set; }
        public int Port { get; internal set; }
        public string Scheme { get; private set; }

        public bool IsUnixPipe
        {
            get
            {
                return Host.StartsWith(Constants.UnixPipeHostPrefix);
            }
        }

        public string UnixPipePath
        {
            get
            {
                Debug.Assert(IsUnixPipe);

                return Host.Substring(Constants.UnixPipeHostPrefix.Length - 1);
            }
        }

        public override string ToString()
        {
            if (IsUnixPipe)
            {
                if (string.IsNullOrEmpty(PathBase))
                {
                    return Scheme.ToLowerInvariant() + "://" + Host.ToLowerInvariant();
                }
                else
                {
                    return Scheme.ToLowerInvariant() + "://" + Host.ToLowerInvariant() + ":" + PathBase.ToLowerInvariant();
                }
            }
            else
            {
                return Scheme.ToLowerInvariant() + "://" + Host.ToLowerInvariant() + ":" + Port.ToString(CultureInfo.InvariantCulture) + PathBase.ToLowerInvariant();
            }
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as ServerAddress;
            if (other == null)
            {
                return false;
            }
            return string.Equals(Scheme, other.Scheme, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Host, other.Host, StringComparison.OrdinalIgnoreCase)
                && Port == other.Port
                && string.Equals(PathBase, other.PathBase, StringComparison.OrdinalIgnoreCase);
        }

        public static ServerAddress FromUrl(string url)
        {
            url = url ?? string.Empty;

            int schemeDelimiterStart = url.IndexOf("://", StringComparison.Ordinal);
            if (schemeDelimiterStart < 0)
            {
                throw new FormatException($"Invalid URL: {url}");
            }
            int schemeDelimiterEnd = schemeDelimiterStart + "://".Length;

            var isUnixPipe = url.IndexOf(Constants.UnixPipeHostPrefix, schemeDelimiterEnd, StringComparison.Ordinal) == schemeDelimiterEnd;

            int pathDelimiterStart;
            int pathDelimiterEnd;
            if (!isUnixPipe)
            {
                pathDelimiterStart = url.IndexOf("/", schemeDelimiterEnd, StringComparison.Ordinal);
                pathDelimiterEnd = pathDelimiterStart;
            }
            else
            {
                pathDelimiterStart = url.IndexOf(":", schemeDelimiterEnd + Constants.UnixPipeHostPrefix.Length, StringComparison.Ordinal);
                pathDelimiterEnd = pathDelimiterStart + ":".Length;
            }

            if (pathDelimiterStart < 0)
            {
                pathDelimiterStart = pathDelimiterEnd = url.Length;
            }

            var serverAddress = new ServerAddress();
            serverAddress.Scheme = url.Substring(0, schemeDelimiterStart);

            var hasSpecifiedPort = false;
            if (!isUnixPipe)
            {
                int portDelimiterStart = url.LastIndexOf(":", pathDelimiterStart - 1, pathDelimiterStart - schemeDelimiterEnd, StringComparison.Ordinal);
                if (portDelimiterStart >= 0)
                {
                    int portDelimiterEnd = portDelimiterStart + ":".Length;

                    string portString = url.Substring(portDelimiterEnd, pathDelimiterStart - portDelimiterEnd);
                    int portNumber;
                    if (int.TryParse(portString, NumberStyles.Integer, CultureInfo.InvariantCulture, out portNumber))
                    {
                        hasSpecifiedPort = true;
                        serverAddress.Host = url.Substring(schemeDelimiterEnd, portDelimiterStart - schemeDelimiterEnd);
                        serverAddress.Port = portNumber;
                    }
                }

                if (!hasSpecifiedPort)
                {
                    if (string.Equals(serverAddress.Scheme, "http", StringComparison.OrdinalIgnoreCase))
                    {
                        serverAddress.Port = 80;
                    }
                    else if (string.Equals(serverAddress.Scheme, "https", StringComparison.OrdinalIgnoreCase))
                    {
                        serverAddress.Port = 443;
                    }
                }
            }

            if (!hasSpecifiedPort)
            {
                serverAddress.Host = url.Substring(schemeDelimiterEnd, pathDelimiterStart - schemeDelimiterEnd);
            }

            if (string.IsNullOrEmpty(serverAddress.Host))
            {
                throw new FormatException($"Invalid URL: {url}");
            }

            // Path should not end with a / since it will be used as PathBase later
            if (url[url.Length - 1] == '/')
            {
                serverAddress.PathBase = url.Substring(pathDelimiterEnd, url.Length - pathDelimiterEnd - 1);
            }
            else
            {
                serverAddress.PathBase = url.Substring(pathDelimiterEnd);
            }

            return serverAddress;
        }

        internal ServerAddress WithHost(string host)
        {
            return new ServerAddress
            {
                Scheme = Scheme,
                Host = host,
                Port = Port,
                PathBase = PathBase
            };
        }
    }
}