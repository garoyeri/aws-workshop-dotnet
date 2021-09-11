namespace AwsHelloWorldWeb.Features.Values
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    public class DatabaseValuesService : IValuesService
    {
        private readonly ValuesContext _context;

        public DatabaseValuesService(ValuesContext context)
        {
            _context = context;
        }
        
        public async Task<string> Get(int id, CancellationToken cancellationToken = default)
        {
            var item = await _context.Values.FindAsync(id);
            return item?.Value;
        }

        public Task<List<string>> List(int maxItems = 100, bool? useBackwardQuery = null, CancellationToken cancellationToken = default)
        {
            return _context.Values
                .OrderBy(v => v.Id)
                .Select(v => v.Value)
                .ToListAsync(cancellationToken);
        }

        public async Task Append(string value, CancellationToken cancellationToken = default)
        {
            await _context.Values.AddAsync(new DatabaseValueItem { Value = value }, cancellationToken);
        }

        public async Task Upsert(int id, string value, CancellationToken cancellationToken = default)
        {
            var found = await _context.Values.FindAsync(id);
            if (found == null)
            {
                await _context.Values.AddAsync(new DatabaseValueItem { Id = id, Value = value }, cancellationToken);
            }
            else
            {
                found.Value = value;
            }
        }

        public async Task Delete(int id, CancellationToken cancellationToken = default)
        {
            var found = await _context.Values.FindAsync(id);
            if (found != null)
            {
                _context.Values.Remove(found);
            }
        }
    }
}