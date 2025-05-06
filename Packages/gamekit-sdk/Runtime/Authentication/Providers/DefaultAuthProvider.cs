using System;
using System.Threading.Tasks;
using Estoty.GameKit.Authentication.Providers;
using Estoty.GameKit.Utility.Responses;

namespace Estoty.Gamekit.Core
{
    public class DefaultAuthProvider : IAuthProvider
    {
        public bool Authenticated => true; // Always true for the default

        public bool Supported => throw new NotImplementedException();

        public Task<Response> Authenticate()
        {
            return Task.FromResult(new Response());
        }
    }
}