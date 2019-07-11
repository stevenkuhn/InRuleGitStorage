﻿using LibGit2Sharp.Handlers;

namespace InRuleContrib.Repository.Storage.Git
{
    /// <summary>
    /// Collection of parameters controlling Clone behavior
    /// </summary>
    public class CloneOptions
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
}
