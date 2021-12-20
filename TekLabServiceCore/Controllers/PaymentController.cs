using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TekLabServiceCore.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class PaymentController : ControllerBase
    {
        private MongoDBWrapper mongoClient { get; set; }

        private readonly ILogger<PaymentController> _logger;

        public PaymentController(MongoDBWrapper mongoDBWrapper,
                                 ILogger<PaymentController> logger)
        {
            mongoClient = mongoDBWrapper;
            _logger = logger;
        }
        [HttpGet]
        public ActionResult CheckOut([FromQuery] string userEmail,string selectedPlan)
        {
            
            var domain = "https://main.d2c99v4gl26kgj.amplifyapp.com";
            var options = new SessionCreateOptions
            {    CustomerEmail = userEmail,
                LineItems = new List<SessionLineItemOptions>
                {
                  new SessionLineItemOptions
                  {
                    // TODO: replace this with the `price` of the product you want to sell
                    Price = GetPriceCode(selectedPlan),
                    Quantity = 1,
                  },
                },
                PaymentMethodTypes = new List<string>
                {
                  "card",
                   "alipay",
                },
                Mode = "payment",
                SuccessUrl = domain + "/success.html",
                CancelUrl = domain + "/cancel.html",
            };
            var service = new SessionService();
            Session session = service.Create(options);
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }


        const string endpointSecret = "whsec_zJbNv1acdoqm2tW7DXWWQ6aeQ0sKEDKR";

        [HttpPost]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], endpointSecret);

                if (stripeEvent.Type == "charge.succeeded")
                {

                    /*Insert in Payments with raw data*/
                    await mongoClient.InsertAsync(json, "paymentsRaw");

                    JObject paymentObject = JObject.Parse(json);
                    var userEmail = paymentObject["data"]["object"]["billing_details"]["email"].ToObject<string>();
                    var _paymentObject = new
                    {
                        billingDeatils = paymentObject["data"]["object"]["billing_details"],
                        amount = paymentObject["data"]["object"]["amount"],
                        transactionId= paymentObject["data"]["object"]["id"],
                        transactionDate = paymentObject["data"]["object"]["created"],
                        receiptUrl = paymentObject["data"]["object"]["receipt_url"],
                        userEmail= userEmail
                    };
                    await mongoClient.InsertAsync(JsonConvert.SerializeObject(_paymentObject), "payments");

                    var userInVisitors = await mongoClient.Find("email", userEmail, "visitors");
                    await mongoClient.InsertAsync(userInVisitors, "users");

                }
                else 
                {
                    Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                }
                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("GetPaymentStatus")]
        public async Task<ActionResult> GetPaymentStatus([FromQuery] string userEmail)
        {
            var isPaymentRecordExists = await mongoClient.IsDocumentExist("userEmail", userEmail, "payments");
            return Ok(isPaymentRecordExists);
            
        }

        private string GetPriceCode(string selectedPlan) {
            var paymentPlans = new
            {
                _1 = "price_1K3ODbB8YTUa63fzWbrNcoVS",
                _2 = "price_1K3OHuB8YTUa63fzAvygLvkR",
                _3 = "price_1K3OISB8YTUa63fzOCcfq6gY",
                _4 = "price_1K3OIyB8YTUa63fzVnxdobtK",

            };
            var priceCode = "";
            if (selectedPlan == "_1")
            {
                priceCode =  paymentPlans._1;
            }
            else if (selectedPlan == "_2")
            {
                priceCode = paymentPlans._2;
            }
            else if (selectedPlan == "_3")
            {
                priceCode = paymentPlans._3;
            }
            else if (selectedPlan == "_4")
            {
                priceCode = paymentPlans._4;
            }
            return priceCode;
        }
    }
}
