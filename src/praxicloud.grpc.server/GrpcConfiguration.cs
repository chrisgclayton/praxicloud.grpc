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
    /// GRPC server configuration
    /// </summary>
    public class GrpcConfiguration : IGrpcConfiguration
    {
        #region Properties
        /// <inheritdoc />
        public bool? UseMultipleForConnectionCounts { get; set; }

        /// <inheritdoc />
        public ushort? MaximumConcurrentConnections { get; set; }

        /// <inheritdoc />
        public ushort? MaximumConcurrentUpgradedConnections { get; set; }

        /// <inheritdoc />
        public TimeSpan? KeepAliveTimeout { get; set; }

        /// <inheritdoc />
        public TimeSpan? RequestHeadersTimeout { get; set; }

        /// <inheritdoc />
        public int? MaximumStreamsPerConnection { get; set; }

        /// <inheritdoc />
        public virtual X509Certificate2 ServerCertificate { get; set; }

        /// <inheritdoc />
        public string UnixSocketPath { get; set; }

        /// <inheritdoc />
        public bool? CheckCertificateRevocation { get; set; }

        /// <inheritdoc />
        public ClientCertificateMode? ClientCertificateMode { get; set; }

        /// <inheritdoc />
        public bool? UseDeveloperExceptionPage { get; set; }

        /// <inheritdoc />
        public bool? UseDeveloperExceptionPageNonProduction { get; set; }

        /// <inheritdoc />
        public bool? EnableNonGrpcWarningMessage { get; set; }

        /// <inheritdoc />
        public string NonGrpcWarningMessage { get; set; }

        /// <inheritdoc />
        public bool? EnableConnectionLogging { get; set; }

        /// <inheritdoc />
        public bool? AllowSelfSigned { get; set; }
        #endregion
    }
}
