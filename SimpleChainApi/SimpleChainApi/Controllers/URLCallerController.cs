using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleChainApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class URLCallerController : ControllerBase
    {
        private const string URL_LIST = "URLS_DEPENDENCIES";

        private readonly ILogger<URLCallerController> _logger;

        private readonly IHttpClientFactory _clientFactory;

        private readonly IConfiguration _configuration;

        public URLCallerController(ILogger<URLCallerController> logger, IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IEnumerable<URLCalled>> GetAsync()
        {
            var urlList = _configuration[URL_LIST];
            _logger.LogInformation("URL dependencies {urlList}", urlList);
            if (!string.IsNullOrWhiteSpace(urlList))
            {
                var client = _clientFactory.CreateClient();
                var result = new List<URLCalled>();
                foreach (var url in urlList.Split(','))
                {
                    var urlCalledResult = new URLCalled { Date = DateTime.Now, URI = url };
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    try
                    {
                        var response = await client.SendAsync(request);
                        urlCalledResult.Success = response.IsSuccessStatusCode;
                        urlCalledResult.StatusCode =  response.StatusCode;
                    }
                    catch (HttpRequestException)
                    {
                        urlCalledResult.Success = false;
                    }
                    result.Add(urlCalledResult);
                }

                return result;
            }

            return Enumerable.Empty<URLCalled>();
        }
    }
}
