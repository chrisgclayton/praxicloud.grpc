// Copyright (c) Christopher Clayton. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace praxicloud.grpc.server
{
    /// <summary>
    /// Startup configuration details for GRPC Kestrel server
    /// </summary>
    public interface IGrpcStartupConfiguration
    {
        #region Properties
        /// <summary>
        /// True if the developer execption page even in production. Defaults to false.
        /// </summary>
        bool? UseDeveloperExceptionPage { get; }

        /// <summary>
        /// True if the developer exception page should be used in development. If UseDeveloperExceptionPage is true this will be ignored and it will be enabled.
        /// </summary>
        bool? UseDeveloperExceptionPageNonProduction { get; }

        /// <summary>
        /// True if the GRPC warning should be sent to non GRPC requests (GET /)
        /// </summary>
        bool? EnableNonGrpcWarningMessage { get; }

        /// <summary>
        /// If not null this string will be used as the warning response.
        /// </summary>
        string NonGrpcWarningMessage { get; }
        #endregion
    }
}
