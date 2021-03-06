﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Moq;

namespace SIO.Identity.Tests
{
    internal class MockIdentityServerInteraction : IIdentityServerInteractionService
    {
        public bool IsValidUrl { get; set; }
        public bool HasAuthorizationContext { get; set; } = true;

        public Task<string> CreateLogoutContextAsync()
        {
            return Task.FromResult("MockLogoutId");
        }

        public Task DenyAuthorizationAsync(AuthorizationRequest request, AuthorizationError error, string errorDescription = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Consent>> GetAllUserConsentsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Grant>> GetAllUserGrantsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<AuthorizationRequest> GetAuthorizationContextAsync(string returnUrl)
        {
            if (HasAuthorizationContext)
                return Task.FromResult(new Mock<AuthorizationRequest>().Object);

            return Task.FromResult((AuthorizationRequest)null);
        }

        public Task<ErrorMessage> GetErrorContextAsync(string errorId)
        {
            throw new NotImplementedException();
        }

        public Task<LogoutRequest> GetLogoutContextAsync(string logoutId)
        {
            return Task.FromResult(new LogoutRequest("", new LogoutMessage())
            {
                ClientId = "clientId"
            });
        }

        public Task GrantConsentAsync(AuthorizationRequest request, ConsentResponse consent, string subject = null)
        {
            return Task.CompletedTask;
        }

        public bool IsValidReturnUrl(string returnUrl)
        {
            return IsValidUrl;
        }

        public Task RevokeTokensForCurrentSessionAsync()
        {
            return Task.CompletedTask;
        }

        public Task RevokeUserConsentAsync(string clientId)
        {
            return Task.CompletedTask;
        }
    }
}
