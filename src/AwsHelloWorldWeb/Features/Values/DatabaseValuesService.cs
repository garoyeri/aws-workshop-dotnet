namespace AwsHelloWorldWeb.Features.Values
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class DatabaseValuesService : IValuesService
    {
        public Task<string> Get(int id, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<string>> List(int maxItems = 100, bool? useBackwardQuery = null, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task Append(string value, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task Upsert(int id, string value, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task Delete(int id, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}