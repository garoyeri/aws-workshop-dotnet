namespace AwsHelloWorldWeb.Features.Values
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    public class ValuesController : ControllerBase
    {
        private readonly ValuesService _values;

        public ValuesController(ValuesService values)
        {
            _values = values;
        }
        
        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            return Ok((await _values.List())
                .OrderBy(r => r.Id)
                .Select(r => r.Value)
                .ToArray());
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get(int id)
        {
            var found = await _values.Get(id);
            if (found == null)
                return NotFound();

            return Ok(found.Value);
        }

        // POST api/values
        [HttpPost]
        public async Task<ActionResult> Post([FromBody]string value)
        {
            await _values.Append(value);

            return Accepted();
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody]string value)
        {
            await _values.Upsert(id, value);

            return Accepted();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _values.Delete(id);

            return Accepted();
        }
    }
}
