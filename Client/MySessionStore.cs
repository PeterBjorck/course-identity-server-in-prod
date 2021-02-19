using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SessionStore
{
    /// <summary>
    /// MySessionStore
    /// 
    /// Custom session store, to hold the tokens in memory instead of storing them inside the Cookie.
    /// 
    /// This provides an abstract storage mechanic to preserve identity information on the server while 
    /// only sending a simple identifier key to the client. This is most commonly used to mitigate issues 
    /// with serializing large identities into cookies.
    /// 
    /// TODO: 
    /// - Needs logic to remove older items, otherwise we might run out of memory here.
    /// - Use the MemoryCache instead of a dictionary?
    /// 
    /// Written by Tore Nestenius to be used in the IdentityServer in production training class.
    /// https://www.edument.se/en/product/identityserver-in-production
    /// 
    /// </summary>
    internal class MySessionStore : ITicketStore
    {
        private readonly Serilog.ILogger _logger;

        private readonly ConcurrentDictionary<string, AuthenticationTicket> mytickets = new();

        public MySessionStore()
        {
            _logger = Log.Logger;
        }

        /// <summary>
        /// Remove the identity associated with the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task RemoveAsync(string key)
        {
            _logger.Debug("MySessionStore.RemoveAsync Key=" + key);

            if (mytickets.ContainsKey(key))
            {
                mytickets.TryRemove(key, out _);
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Tells the store that the given identity should be updated.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            _logger.Debug("MySessionStore.RenewAsync Key=" + key + ", ticket = " + ticket.AuthenticationScheme);

            mytickets[key] = ticket;

            return Task.FromResult(false);
        }


        /// <summary>
        /// Retrieves an identity from the store for the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            _logger.Error("MySessionStore.RetrieveAsync Key=" + key);

            if (mytickets.ContainsKey(key))
            {
                var ticket = mytickets[key];
                return Task.FromResult(ticket);
            }
            else
            {
                return Task.FromResult((AuthenticationTicket)null!);
            }
        }


        /// <summary>
        /// Store the identity ticket and return the associated key.
        /// </summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            //Only add one at the time to avoid race conditions
            lock(this)
            {

                //Make sure the key is does not already exist in the dictionary
                bool result = false;
                string key;
                do
                {
                    key = Guid.NewGuid().ToString();
                    result = mytickets.TryAdd(key, ticket);
                } while (result == false);

                string username = ticket?.Principal?.Identity?.Name ?? "Unknown";
                _logger.Debug("MySessionStore.StoreAsync ticket=" + username + ", key=" + key);

                return Task.FromResult(key);
            }
  
        }
    }
}
