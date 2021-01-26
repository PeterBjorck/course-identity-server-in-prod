using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Quickstart.Error
{
    public interface IDummy
    {
        //dummy interface to trigger framework error
    }

   [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorController : Controller
    {
        // /error/error1
        // Triggers a InvalidOperationException exception
        public IActionResult Error1(IDummy dummy)
        {
            return View();
        }

        // /error/error2
        // Triggers a InvalidOperationException: The view 'Error2' was not found.
        public IActionResult Error2()
        {
            //View not found error
            return View();
        }

        // /error/error3
        // Trigger an exception in the action method
        public IActionResult Error3()
        {
            throw new Exception("Houston, we have a problem");
        }

        // /error/servererror
        // Returns a 500 error code
        public IActionResult ServerError()
        {
            return StatusCode(500);
        }

        // /error/notfound
        // Returns a 404 error code
        public IActionResult NotFound()
        {
            return StatusCode(404);
        }

        // /error/noaccess
        // Returns a 401 error code
        public IActionResult NoAccess()
        {
            return StatusCode(401);
        }
    }

    [ApiController]
    [Route("/api/error1")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ApiErrorController : Controller
    {
        // /api/error1
        // Throw a framework error because IDummy does not have a concrete type
        public ApiErrorController(IDummy dummy)
        {
        }

        public IActionResult Error1()
        {
            //We never get to this method (it will crash in the constructor)
            return Ok("Error1");
        }
    }


    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ApiError2Controller : Controller
    {
        // Throw an exception
        [Route("/api/error2")]
        public IActionResult Error2()
        {
            throw new Exception("test error occured!");
        }

        // Return a sample API problem response
        [Route("/api/error3")]
        public IActionResult Error3()
        {
            // See the "Problem Details for HTTP APIs" standard for details
            // at https://tools.ietf.org/html/rfc7807
            return Problem(title: "Houston, we have a problem",
                           detail: "Please contact support");
        }
    }
}
