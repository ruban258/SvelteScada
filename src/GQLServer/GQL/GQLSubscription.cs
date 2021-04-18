using GQLServer.infra;
using GQLServer.Models;
using HotChocolate;
using HotChocolate.Types;

namespace GQLServer.GQL
{
    public class GQLSubscription
    {
        private readonly UAClient _uaClient;

        public GQLSubscription(UAClient uAClient)
        {
            _uaClient = uAClient;
        }
        [Topic]
        [Subscribe]
        public Tag OnTagUpdated([EventMessage] Tag node)
        {
            return node;
        }
    }
}