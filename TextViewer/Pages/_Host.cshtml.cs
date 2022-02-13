using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Components;
using TextViewer.Classes;
using Microsoft.AspNetCore.Mvc;

namespace TextViewer.Pages
{
    public class HostModel : PageModel
    {
        public IActionResult OnGet()
        {
            string UserAgent = Request.Headers["User-Agent"];
            // Note that the RemoteIpAddress property returns an IPAdrress object 
            // which you can query to get required information. Here, however, we pass 
            // its string representation
            string IPAddress = HttpContext.Connection.RemoteIpAddress.ToString();
            Logic.Log("Connection from: " + IPAddress + ". Agent is " + UserAgent + ". Trying to open " + Request.Path + ".");
            string result = Logic.RouteFilter(UserAgent, IPAddress, Request);
            if (string.IsNullOrWhiteSpace(result))
                return Page();
            else
                return Redirect(result);
        }
    }
}
