using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using PIMS.Core.Models;

// Leveraging existence of Microsoft's embedded authorization server: Owin/Katana OAuth2 middleware. If necessary
// at a later point, we'll migrate to an external authorization server, e.g., Thinktecture. 


namespace PIMS.Core.Security
{
    public class KatanaAuthorizationServer : OAuthAuthorizationServerProvider
    {
        private readonly string _publicClientId;
        private readonly Func<UserManager<ApplicationUser>> _userManagerFactory;

        public KatanaAuthorizationServer(string publicClientId, Func<UserManager<ApplicationUser>> userManagerFactory)
        {
            if (publicClientId == null) {
                throw new ArgumentNullException("publicClientId");
            }

            if (userManagerFactory == null) {
                throw new ArgumentNullException("userManagerFactory");
            }

            _publicClientId = publicClientId;
            _userManagerFactory = userManagerFactory;
        }

        public KatanaAuthorizationServer()
        {
            
        }



        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            using (var userManager = _userManagerFactory())
            {
                // Validate the username and password credentials.
                var user = await userManager.FindAsync(context.UserName, context.Password);

                // Avoid error:  "No 'Access-Control-Allow-Origin' header is present on the requested resource" via
                //               specifying CLIENT Url with port. '[EnableCorsAttribute]' annotation must be added
                //               to all applicable controllers.
                context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[]{"http://localhost:5969/"});

                if (user == null || (string.IsNullOrWhiteSpace(context.UserName) || string.IsNullOrWhiteSpace(context.Password) ))
                {
                    context.SetError("invalid_grant", "The user name and/or password is invalid.");
                    context.Rejected();
                    return; 
                }

                // Create a ClaimsIdentity that represents the authenticated user; any claims added describe the identity of
                // the user, e.g. his user id (the ‘sub’ claim) or roles he is a member of, and becomes part of the token.
                // ASP.NET Identity supports claims-based authentication, where the user’s identity is represented as a set of claims.
                // Claims allow developers to be a lot more expressive in describing a user’s identity than roles allow. Whereas role 
                // membership is just a boolean (member or non-member), a claim can include rich information about the user’s
                // identity and membership.
                var oAuthClaimsIdentity = await userManager.CreateIdentityAsync(user, context.Options.AuthenticationType);
                oAuthClaimsIdentity.AddClaim(new Claim("sub", context.UserName)); // RPA: redundant?
                oAuthClaimsIdentity.AddClaim(new Claim("role", "user"));          // RPA: added per D.Baier article
                var authSessionValues = CreateProperties(user.UserName);
                var ticket = new AuthenticationTicket(oAuthClaimsIdentity, authSessionValues);
                context.Validated(ticket);
            }
        }
        
        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            // Key/Value pairs.
            foreach (var property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // Resource owner password credentials does not provide a client ID to be validated.
            if (context.ClientId == null) {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context) {
            if (context.ClientId == _publicClientId) {
                var expectedRootUri = new Uri(context.Request.Uri, "/");

                if (expectedRootUri.AbsoluteUri == context.RedirectUri) {
                    context.Validated();
                }
            }

            return Task.FromResult<object>(null);
        }

        public static AuthenticationProperties CreateProperties(string userName)
        {
            IDictionary<string, string> data = new Dictionary<string, string>
            {
                { "userName", userName }
            };
            return new AuthenticationProperties(data);
        }
    }
}
