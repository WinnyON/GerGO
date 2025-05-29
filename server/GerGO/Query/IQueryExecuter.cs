using GerGO.DataAcces.MetaData;
using GerGO.DataAcces.StoredData;
using GerGO.Models;

namespace GerGO.Query
{
    interface IQueryExecuter
    {
        List<string> ExecuteQuery(ref SelectData selectData);
    }
}
