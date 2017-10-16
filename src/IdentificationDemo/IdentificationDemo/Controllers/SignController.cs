using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Idfy.Identification.Client;
using Idfy.Identification.Models;
using Newtonsoft.Json;
using Unipluss.Sign.Client.Models;
using Unipluss.Sign.ExternalContract.Entities;
using IdentityProviderType = Unipluss.Sign.ExternalContract.Entities.SignereID.IdentityProviderType;
using Languages = Unipluss.Sign.ExternalContract.Entities.Languages;

namespace IdentificationDemo.Controllers
{
    public class SignController : Controller
    {
        private Unipluss.Sign.Client.Client client = new Unipluss.Sign.Client.Client("https://testapi.signere.no",
            new Guid("YOUR_ACCOUNT_ID"), "YOUR API KEY",false,true,true
           );

        private const string baseUrl = "https://localhost:44334";

        

        public async Task<ActionResult> Index()
        {

            var response = client.CreateExternalSign(new NewExternalSignDocument()
                {
                    CreatePADES = true,
                    CreatedByApplication = "Idfy hackaton",
                    Description = "A nice demo document to sign for the Hackton",
                    ExternalDocumentId = Guid.NewGuid().ToString(),
                    Domain = "localhost:44334",
                    ExternalRef = Guid.NewGuid().ToString(),
                    DocumentType = DocumentType.PDF,
                    IdentityProvider = IdentityProviderType.NO_BANKID_WEB,
                    Language = Languages.EN,
                    ReturnUrlError = baseUrl + Url.Action("Error") + $"?statuscode=[0]",
                    ReturnUrlSuccess = baseUrl + Url.Action("Success")+"signeeRefId=[0]&externalId=[1]",
                    ReturnUrlUserAbort = baseUrl + Url.Action("Abort"),
                    Title = "Hackton demo",
                    UseIframe = true,
                    UseWebMessaging = true,
                    PushNotificationUrl = "https://requestb.in/12exktl1?documentid=[0]&providerid=[1]&operation=[2]&externaldocumentid=[3]&signeeref=[4]&externaluniqueid=[5]",
                    SigneeRefs = new List<ExternalSigneeRef>()
                    {
                        new ExternalSigneeRef()
                        {
                            FirstName = "Rune",
                            LastName = "Synnevåg",
                            UniqueRef = Guid.NewGuid(),
                            ExternalSigneeId = Guid.NewGuid().ToString()
                        }
                    },                    
               
        }, System.IO.Path.Combine(Server.MapPath("~/App_data/"), "samplecontract.pdf"));

            Response.Cookies.Add(new HttpCookie("docid",response.DocumentId.ToString()));

            ViewBag.Url = response.CreatedSigneeRefs?.FirstOrDefault()?.SignUrl;
            ViewBag.DocumentId = response.DocumentId;
            return View();
        }

        public ActionResult Abort()
        {
            ViewBag.Message = "Aborted";

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

        public async Task<ActionResult> Success(string externalId,string signeeRefId)
        {
            var documentId = Request.Cookies.Get("docid")?.Value;
            var response = client.GetDocument(new Guid(documentId));
            ViewBag.Json = Newtonsoft.Json.JsonConvert.SerializeObject(response,Formatting.Indented);

            ViewBag.DownloadLink = Url.Action("DownloadFile", new {id = documentId});

            return View();
        }

        public ActionResult DownloadFile(string id)
        {
            var response = client.DownloadSignedPDFDocument(new Guid(id));
            return new FileContentResult(response,"application/pdf");
        }
    }
}