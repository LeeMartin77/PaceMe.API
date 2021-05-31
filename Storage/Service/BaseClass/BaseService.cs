using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos.Table;
using PaceMe.Storage.Utilities;
using System;

namespace PaceMe.Storage.Service 
{
    public interface IBaseService<T, ID>
    {
        Task<ID> Create(T record);
        Task<T> Get(ID recordId);
        Task Update(T record);
        Task Delete(ID record);
    }
}