using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentAPI.Controllers
{
    public class PaymentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Will return the user name and the associated claims
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public IActionResult Get()
        {
            var result = new Result();

            if (User != null)
            {
                result.Name = User.Identity?.Name ?? "Unknown Name";
                result.Claims = (from c in User.Claims select c.Type + ":" + c.Value).ToList();
            }

            return new JsonResult(result);
        }
    }
    public class Result
    {
        public string? Name { get; set; }
        public List<string>? Claims { get; set; }
    }
}
