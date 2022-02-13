using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TextViewer.Classes;
using Microsoft.AspNetCore.Components;

namespace TextViewer.Pages
{
    public class CookieModel : PageModel
    {
        public IActionResult OnGet()
        {
            Logic.Log("Cookie procedure started.");
            var claimsIdentity = new ClaimsIdentity(new List<Claim> {
                    new Claim(ClaimTypes.Name, "userKeBl")}, CookieAuthenticationDefaults.AuthenticationScheme);

            try
            {
                HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = true });
            }
            catch(Exception ex)
            {
                Logic.Log("Exception triggered for cookie. Details: " + ex.Message);
            }

            try
            {
                System.IO.File.WriteAllText(Path.Combine("data", "lock.d"), HttpContext.Connection.RemoteIpAddress.ToString());
            }
            catch(Exception ex)
            {
                Logic.Log("Exception triggered writing ip after cookie. Details: " + ex.Message);
            }
            Logic.Log("Cookie procedure success. Wait for sleep.");
            Thread.Sleep(1500);
            return Redirect("/app/35DC6E98-334D-4F26-A5DD-07AB233EA3A8");
        }

    }
}
