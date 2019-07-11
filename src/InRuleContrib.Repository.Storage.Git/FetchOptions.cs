﻿using LibGit2Sharp.Handlers;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("InRuleContrib.Repository.Storage.Git.Tests")]

namespace InRuleContrib.Repository.Storage.Git
{
    /// <summary>
    /// Collection of parameters controlling Fetch behavior
    /// </summary>
    public class FetchOptions
    {
        /// <summary>
        /// Handler to generate LibGit2Sharp.Credentials for authentication.
        /// </summary>
        public CredentialsHandler CredentialsProvider
        {
            get;
            set;
        }

        /// <summary>
        /// This handler will be called to let the user make a decision on whether to allow the connection to preoceed based on the certificate presented by the server.
        /// </summary>
        public CertificateCheckHandler CertificateCheck
        {
            get;
            set;
        }
    }

    public class PullOptions
    {

    }
}
