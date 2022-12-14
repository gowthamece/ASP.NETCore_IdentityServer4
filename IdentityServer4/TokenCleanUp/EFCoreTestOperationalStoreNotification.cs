using System.Collections.Generic;
using System;
using IdentityServer4.EntityFramework;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;

namespace IdentityServer4Sample.TokenCleanUp
{
    public class EFCoreTestOperationalStoreNotification: IOperationalStoreNotification
    {
        public EFCoreTestOperationalStoreNotification()
        {
            Console.WriteLine("Sql Server Notification ctor");
        }

        public Task PersistedGrantsRemovedAsync(
            IEnumerable<PersistedGrant> persistedGrants)
        {
            foreach (var grant in persistedGrants)
            {
                Console.WriteLine("cleaned from SQL Server: " + grant.Type);
            }
            return Task.CompletedTask;
        }
    }
}
