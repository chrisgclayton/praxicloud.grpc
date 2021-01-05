// Copyright (c) Christopher Clayton. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace praxicloud.grpc.server
{
    #region Using Clauses
    using System;
    using System.Security.Cryptography.X509Certificates;
    using Microsoft.AspNetCore.Server.Kestrel.Https;
    #endregion

    /// <summary>
    /// The configuration for a GRPC server
    /// </summary>
    public interface IGrpcConfiguration : IGrpcStartupConfiguration
    {
        #region Properties
        /// <summary>
        /// True if the connection count numbers specified should be multiplied by the machines core count. Defaults to false.
        /// </summary>
        bool? UseMultipleForConnectionCounts { get; }

        /// <summary>
        /// The maximum number of concurrent connections, or multiplier by core count based on UseMultipleForConnectionCounts value, allowed to the server. 
        /// </summary>
        ushort? MaximumConcurrentConnections { get; }

        /// <summary>
        /// The maximum number of concurrent connections, or multiplier by core count based on UseMultipleForConnectionCounts value, that are upgraded to HTTP2 or WebSockets allowed to the server. 
        /// </summary>
        ushort? MaximumConcurrentUpgradedConnections { get; }

        /// <summary>
        /// Gets or sets the keep alive timeout, defaulting to 2 minutes
        /// </summary>
        TimeSpan? KeepAliveTimeout { get; }

        /// <summary>
        /// The maximum time the server will receive headers for, defaults to 30 seconds
        /// </summary>
        TimeSpan? RequestHeadersTimeout { get; }

        /// <summary>
        /// The maximum number of streams allowed per HTTP/2 connection, excess streams will be refused. This must be greater than 0. Defaults to 100
        /// </summary>
        int? MaximumStreamsPerConnection { get; }

        /// <summary>
        /// The server certificate to use for HTTPS communications
        /// </summary>
        X509Certificate2 ServerCertificate { get; }

        /// <summary>
        /// If set the server will use a unix socket to listen on for improved performance on Linux with Nginx
        /// </summary>
        string UnixSocketPath { get; }

        /// <summary>
        /// Determines whether the certificate revocation list is checked during authentication. Defaults to true.
        /// </summary>
        bool? CheckCertificateRevocation { get; }

        /// <summary>
        /// Specifies the client certificate requirements for the HTTPS connection, defaulting to NoCertificate
        /// </summary>
        ClientCertificateMode? ClientCertificateMode { get; }

        /// <summary>
        /// Used for debugging purposes to write verbose data from the connection to the debug stream. Defaults to false
        /// </summary>
        bool? EnableConnectionLogging { get; }

        /// <summary>
        /// True if the Kestrel Server should accept self signed certificates
        /// </summary>
        bool? AllowSelfSigned { get; }
        #endregion
    }
}
