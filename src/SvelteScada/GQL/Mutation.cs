using System.Threading.Tasks;
using GQLServer.Models;

namespace GQLServer.GQL
{
    public class Mutation
    {
        public Tag MutateTag(Tag input)
        {
            var tag = new Tag{ TagName = input.TagName};
               return tag;
        }
        
    }
    
}