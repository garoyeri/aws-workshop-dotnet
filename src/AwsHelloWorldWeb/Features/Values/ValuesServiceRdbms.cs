namespace AwsHelloWorldWeb.Features.Values
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class ValuesServiceRdbms : IValuesService
    {
        public Task<ValueItem> Get(int id, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<ValueItem>> List(int maxItems = 100, bool? useBackwardQuery = null, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<ValueItem> Append(string value, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<ValueItem> Upsert(int id, string value, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task Delete(int id, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}