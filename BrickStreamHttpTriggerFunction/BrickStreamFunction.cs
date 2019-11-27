using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BrickStreamHttpTriggerFunction
{
    public static class BrickStreamFunction
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [EventHub("", Connection = "eventhub")] IAsyncCollector<EventData> output,
            ILogger log)
        {
            log.LogInformation("Brickstream event received");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            var doc = new XmlDocument();
            doc.LoadXml(requestBody);
            var json = JsonConvert.SerializeXmlNode(doc);

            dynamic data = JsonConvert.DeserializeObject(json);

            // Adds message body to output event hub
            var eventDataOut = new EventData(Encoding.UTF8.GetBytes(data.ToString()));
            await output.AddAsync(eventDataOut);

            return data != null
                ? (ActionResult)new OkObjectResult($"Event sent {data.ToString()}")
                : new BadRequestObjectResult("Log what went wrong");
        }
    }
}
