using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GQLServer.Data;
using GQLServer.infra;
using GQLServer.Models;
using HotChocolate;
using Microsoft.EntityFrameworkCore;

namespace GQLServer.GQL
{
    public class Query
    {
        private readonly UAClient _uaClient;

        public Query(UAClient uAClient)
        {
            _uaClient = uAClient;
        }
        public async Task<ObservableCollection<Tag>> GetTags()
        {
            if(_uaClient.Session.Connected)
            {
                if(_uaClient.Tags != null)
                {
                    return _uaClient.Tags;
                }
                
            }
            else
            {
               await _uaClient.ConnectAsync();
               return _uaClient.Tags;
            }
            return new ObservableCollection<Tag>();
        }
        public IQueryable<Recepie> GetRecepies([Service] AppDbContext context)
        {
            return context.Recepies.Include(recepie => recepie.Steps);       
        }
    }
}
