using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Idfy.Identification.Client;
using Idfy.Identification.Models;
using Newtonsoft.Json;

namespace IdentificationDemo.Controllers
{
    public class HomeController : Controller
    {
        private Idfy.Identification.Client.IdentificationClient client = new IdentificationClient(
            new Guid("f753d8b0-88dc-4f2f-a49d-a76200b8d1cf"),
            "7d84aacb-41a9-43c4-b59e-ab5c6451a1f0", "", "identify",
            IdentificationClient.Environment.TEST);

        private const string baseUrl = "https://localhost:44334";

        public async Task<ActionResult> Index()
        {

            var response = await client.Create(new CreateIdentificationRequest()
            {
                ExternalReference = Guid.NewGuid().ToString(),
                ReturnUrls = new ReturnUrls()
                {
                    Abort = baseUrl+Url.Action("About"),
                    Error = baseUrl+Url.Action("Error")+$"?statuscode=[0]",
                    Success = baseUrl+Url.Action("Success")+"?requestId=[1]",
                },
                iFrame = new iFrameSettings()
                {
                    Domain = baseUrl.Replace("https://",""),

                }
            });

            ViewBag.Url = response.Url;
            ViewBag.RequestId = response.RequestId;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Aborted";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Error(string statuscode)
        {
            return new ContentResult()
            {
                Content = statuscode,
                ContentType = "text/plain",
            };
        }

        public async Task<ActionResult> Success(string requestId)
        {
            var response = await client.GetResponse(requestId,true);
            ViewBag.Json = Newtonsoft.Json.JsonConvert.SerializeObject(response,Formatting.Indented);

            return View();
        }
    }
}