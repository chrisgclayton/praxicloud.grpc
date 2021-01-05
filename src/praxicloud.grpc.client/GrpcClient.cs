// Copyright (c) Christopher Clayton. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace praxicloud.grpc.client
{
    #region Using Clauses
    using Grpc.Core;
    using Grpc.Net.Client;
    using praxicloud.core.security;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Security;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    #endregion

    /// <summary>
    /// A helper class for GRPC clients 
    /// </summary>
    public static class GrpcClient
    {
        #region Methods
        /// <summary>
        /// Gets a new channel from for communications with the server URI identified
        /// </summary>
        /// <param name="serverUri">The URI of the hosting GRPC server</param>
        /// <param name="channelOptions">The options used to configure the channel</param>
        /// <returns></returns>
        public static GrpcChannel GetChannel(Uri serverUri, GrpcChannelOptions channelOptions = null)
        {
            return GrpcChannel.ForAddress(serverUri, channelOptions);
        }

        /// <summary>
        /// Converts a dictionary to a metadata object. If null or empty the Metadata object returned will be null;
        /// </summary>
        /// <param name="metadata">The metadata to be added for the call. Only byte[] and string types are used without conversion, all others use the ToString() function to present as a string.</param>
        /// <returns>A populated metadata instance or null if empty</returns>
        public static Metadata GetCallMetadata(Dictionary<string, object> metadata = null)
        {
            Metadata grpcMetadata = null;

            if ((metadata?.Count ?? 0) > 0)
            {
                grpcMetadata = new Metadata();

                foreach (var pair in metadata)
                {
                    if (pair.Value is byte[] arrayValue)
                    {
                        grpcMetadata.Add(pair.Key, arrayValue);
                    }
                    else if (pair.Value is string stringValue)
                    {
                        grpcMetadata.Add(pair.Key, stringValue);
                    }
                    else
                    {
                        grpcMetadata.Add(pair.Key, pair.Value.ToString());
                    }
                }
            }

            return grpcMetadata;
        }

        /// <summary>
        /// Creates a populated call options instance based on the provided parameters
        /// </summary>
        /// <param name="metadata">The metadata to be added for the call.</param>
        /// <param name="credentials">The credentials associated with the call</param>
        /// <param name="timeout">The timespan to add to the current UTC time to act as the call deadline</param>
        /// <param name="cancellationToken">A best effort cancellation token</param>
        /// <returns>A call options instace</returns>
        public static CallOptions GetCallOptions(Metadata metadata = null, CallCredentials credentials = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var options = new CallOptions();

            if (metadata != null) options = options.WithHeaders(metadata);
            if (cancellationToken != default) options = options.WithCancellationToken(cancellationToken);
            if (timeout.HasValue) options = options.WithDeadline(DateTime.UtcNow.Add(timeout.Value));
            if (credentials != null) options = options.WithCredentials(credentials);

            return options;
        }

        /// <summary>
        /// Builds the common GRPC channel options for TLS 1.2 and HTTP/2
        /// </summary>
        /// <param name="maximumConcurrentConnections">The maximum number of concurrent connections allowed to a host</param>
        /// <param name="remoteCertificateValidation">The remote certificate callback in use</param>
        /// <param name="webProxy">The web proxy in use if one is required</param>
        /// <param name="credentials">Credentials associated with the channel if provided</param>
        /// <returns>A populated channel options instance</returns>
        public static GrpcChannelOptions GetChannelOptions(int maximumConcurrentConnections, Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> remoteCertificateValidation, IWebProxy webProxy = null, ChannelCredentials credentials = null)
        {
            Guard.NotLessThan(nameof(maximumConcurrentConnections), maximumConcurrentConnections, 1);
            Guard.NotNull(nameof(remoteCertificateValidation), remoteCertificateValidation);

            var handler = new HttpClientHandler
            {
                MaxConnectionsPerServer = maximumConcurrentConnections,
                SslProtocols = SslProtocols.Tls12,
                ServerCertificateCustomValidationCallback = remoteCertificateValidation
            };

            if (webProxy != null) handler.Proxy = webProxy;

            GrpcChannelOptions options = new GrpcChannelOptions
            {
                Credentials = credentials,
                HttpHandler = handler
            };

            return options;
        }

        /// <summary>
        /// Builds the common GRPC channel options for TLS 1.2 and HTTP/2
        /// </summary>
        /// <param name="maximumConcurrentConnections">The maximum number of concurrent connections allowed to a host</param>
        /// <param name="webProxy">The web proxy in use if one is required</param>
        /// <param name="allowSelfSignedCertificates">True if self signed permissions should be allowed, ignored if remoteCertificateValidation is provided</param>
        /// <param name="allowHostNameMismatch">True to allow the host name on the certificate mismatch when allow self signed is true. Not recommended for production.</param>
        /// <param name="credentials">Credentials associated with the channel if provided</param>
        /// <returns>A populated channel options instance</returns>
        public static GrpcChannelOptions GetChannelOptions(int maximumConcurrentConnections, bool allowSelfSignedCertificates, bool allowHostNameMismatch, IWebProxy webProxy = null, ChannelCredentials credentials = null)
        {
            Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> validationPolicy = null;

            if (!allowSelfSignedCertificates)
            {
                validationPolicy = ValidateServerCertificate;
            }
            else if (allowHostNameMismatch)
            {
                validationPolicy = ValidateServerCertificateAllowSelfSignedAndHostMismatch;
            }
            else
            {
                validationPolicy = ValidateServerCertificateAllowSelfSigned;
            }

            return GetChannelOptions(maximumConcurrentConnections, validationPolicy, webProxy, credentials);
        }


        /// <summary>
        /// Performs client certificate validation
        /// </summary>
        /// <param name="certificate">The client provided certificate to validate</param>
        /// <param name="chain">The certificate chain that is associated with the client certificate</param>
        /// <param name="policyErrors">A list of policy errors</param>
        /// <param name="requestMessage">The request message associated with the validation</param>
        /// <returns>True if the certificate should be accepted</returns>
        public static bool ValidateServerCertificate(HttpRequestMessage requestMessage, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return policyErrors == SslPolicyErrors.None;
        }

        /// <summary>
        /// Performs client certificate validation, allowing self signed
        /// </summary>
        /// <param name="certificate">The client provided certificate to validate</param>
        /// <param name="chain">The certificate chain that is associated with the client certificate</param>
        /// <param name="policyErrors">A list of policy errors</param>
        /// <param name="requestMessage">The request message associated with the validation</param>
        /// <returns>True if the certificate should be accepted</returns>
        public static bool ValidateServerCertificateAllowSelfSigned(HttpRequestMessage requestMessage, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            var isValid = policyErrors == SslPolicyErrors.None;

            if (!isValid)
            {
                // If the only errors are chain specific errors review the chain
                if ((policyErrors & SslPolicyErrors.RemoteCertificateChainErrors) != 0 && (policyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) == 0 && (policyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) == 0)
                {
                    if (chain == null)
                    {
                        // At this point the only errors in the certificate chain are untrusted root errors for self-signed certificates
                    }
                    else
                    {
                        var isChainErrorFound = false;

                        foreach (var status in chain.ChainStatus)
                        {
                            // If there is an error that has the subject of the certificate being the same as the issuer and it is untrusted root error, with no others it is valid, otherwise an addition chain error was found
                            if (!((certificate.Subject == certificate.Issuer) && (status.Status == X509ChainStatusFlags.UntrustedRoot)) && (status.Status != X509ChainStatusFlags.NoError))
                            {
                                isChainErrorFound = true;
                            }
                        }

                        isValid = !isChainErrorFound;
                    }
                }
            }

            return isValid;
        }

        /// <summary>
        /// Performs client certificate validation, allowing self signed
        /// </summary>
        /// <param name="certificate">The client provided certificate to validate</param>
        /// <param name="chain">The certificate chain that is associated with the client certificate</param>
        /// <param name="policyErrors">A list of policy errors</param>
        /// <param name="requestMessage">The request message associated with the validation</param>
        /// <returns>True if the certificate should be accepted</returns>
        public static bool ValidateServerCertificateAllowSelfSignedAndHostMismatch(HttpRequestMessage requestMessage, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            var isValid = policyErrors == SslPolicyErrors.None;

            if (!isValid)
            {
                // If the only errors are chain specific errors review the chain
                if ((policyErrors & SslPolicyErrors.RemoteCertificateChainErrors) != 0 && (policyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) == 0)
                {
                    if (chain == null)
                    {
                        // At this point the only errors in the certificate chain are untrusted root errors for self-signed certificates
                    }
                    else
                    {
                        var isChainErrorFound = false;

                        foreach (var status in chain.ChainStatus)
                        {
                            // If there is an error that has the subject of the certificate being the same as the issuer and it is untrusted root error, with no others it is valid, otherwise an addition chain error was found
                            if (!((certificate.Subject == certificate.Issuer) && (status.Status == X509ChainStatusFlags.UntrustedRoot)) && (status.Status != X509ChainStatusFlags.NoError))
                            {
                                isChainErrorFound = true;
                            }
                        }

                        isValid = !isChainErrorFound;
                    }
                }
            }

            return isValid;
        }
        #endregion
    }
}
