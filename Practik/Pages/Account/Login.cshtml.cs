using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Praktik.Application.Services;

namespace Praktik.Pages.Account;

public class LoginModel : PageModel
{
    private readonly AuthService _auth;
    public LoginModel(AuthService auth) => _auth = auth;

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var (ok, err, _) = await _auth.ValidateUserAsync(Input.Login, Input.Password);
        if (!ok) { ErrorMessage = err; return Page(); }
        return RedirectToPage("/Documents/Index");
    }
}