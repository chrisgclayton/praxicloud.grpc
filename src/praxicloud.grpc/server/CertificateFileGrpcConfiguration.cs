// Copyright (c) Christopher Clayton. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace praxicloud.grpc.server
{
    #region Using Clauses
    using System.Security.Cryptography.X509Certificates;
    #endregion

    /// <summary>
    /// GRPC server configuratio that uses a certificate file found locally
    /// </summary>
    public class CertificateFileGrpcConfiguration : GrpcConfiguration
    {
        /// <summary>
        /// The name of the certificate file
        /// </summary>
        public string CertificateFileName { get; set; }

        /// <summary>
        /// The password for the certificate file
        /// </summary>
        public string CertificateFilePassword { get; set; }

        /// <inheritdoc />
        public override X509Certificate2 ServerCertificate 
        { 
            get => string.IsNullOrWhiteSpace(CertificateFilePassword) ? new X509Certificate2(CertificateFileName) : new X509Certificate2(CertificateFileName, CertificateFilePassword); 
            set { } 
        }
    }
}
