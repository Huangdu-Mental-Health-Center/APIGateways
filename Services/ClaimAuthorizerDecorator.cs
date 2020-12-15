using Ocelot.Authorization;
using System.Collections.Generic;
using System.Security.Claims;
using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Responses;

namespace APIGateways.Services
{
    public class ClaimAuthorizerDecorator : IClaimsAuthorizer
    {
        private readonly ClaimsAuthorizer _Authorizer;

        public ClaimAuthorizerDecorator(ClaimsAuthorizer Authorizer)
        {
            _Authorizer = Authorizer;
        }

        public Response<bool> Authorize(ClaimsPrincipal claimsPrincipal,
                                        Dictionary<string, string> routeClaimsRequirement,
                                        List<PlaceholderNameAndValue> urlPathPlaceholderNameAndValues)
        {
            var newRouteClaimsRequirement = new Dictionary<string, string>();
            foreach (var kvp in routeClaimsRequirement)
            {
                if (kvp.Key.StartsWith("http///"))
                {
                    var key = kvp.Key.Replace("http///", "http://");
                    newRouteClaimsRequirement.Add(key, kvp.Value);
                }
                else
                {
                    newRouteClaimsRequirement.Add(kvp.Key, kvp.Value);
                }
            }

            return _Authorizer.Authorize(claimsPrincipal, newRouteClaimsRequirement, urlPathPlaceholderNameAndValues);
        }
    }
}
