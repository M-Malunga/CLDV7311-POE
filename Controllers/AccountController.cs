using ST10296771_CLDV7311_POE.Data;
using ST10296771_CLDV7311_POE.Models;
using ST10296771_CLDV7311_POE.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ST10296771_CLDV7311_POE.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AccountController> _logger;
        private readonly IUserService _userService;
        private readonly IPasswordHasher _passwordHasher;

        public AccountController(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AccountController> logger,
            IUserService userService,
            IPasswordHasher passwordHasher)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _userService = userService;
            _passwordHasher = passwordHasher;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _logger.LogInformation($"Login attempt - Role: {model.Role}, Username/Email: {model.UsernameOrEmail}");

            bool isValid = false;
            int userId = 0;
            string userName = "";

            switch (model.Role)
            {
                case "Customer":
                    var customer = await _context.Users
                        .FirstOrDefaultAsync(u => u.Username == model.UsernameOrEmail || u.Email == model.UsernameOrEmail);

                    if (customer != null)
                    {
                        isValid = _userService.ValidatePassword(model.Password, customer.PasswordHash);
                        if (isValid)
                        {
                            userId = customer.UserId;
                            userName = customer.Username;
                        }
                    }
                    break;

                case "Employee":
                    var employee = await _context.Employees
                        .FirstOrDefaultAsync(e => e.Username == model.UsernameOrEmail || e.Email == model.UsernameOrEmail);

                    if (employee != null)
                    {
                        isValid = _userService.ValidatePassword(model.Password, employee.PasswordHash);
                        if (isValid)
                        {
                            userId = employee.EmployeeId;
                            userName = employee.Username;
                        }
                    }
                    break;

                case "Admin":
                    var admin = await _context.Administrators
                        .FirstOrDefaultAsync(a => a.Username == model.UsernameOrEmail || a.Email == model.UsernameOrEmail);

                    if (admin != null)
                    {
                        isValid = _userService.ValidatePassword(model.Password, admin.PasswordHash);
                        if (isValid)
                        {
                            userId = admin.AdminId;
                            userName = admin.Username;
                        }
                    }
                    break;
            }

            if (isValid)
            {
                _httpContextAccessor.HttpContext.Session.SetInt32("UserId", userId);
                _httpContextAccessor.HttpContext.Session.SetString("UserRole", model.Role);
                _httpContextAccessor.HttpContext.Session.SetString("UserName", userName);

                _logger.LogInformation($"Successful login for {model.Role}: {userName}");

                if (model.Role == "Admin")
                    return RedirectToAction("ManageCustomers", "Admin");
                else if (model.Role == "Employee")
                    return RedirectToAction("Index", "BookingRequests");
                else
                    return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid username/email or password.");
            return View(model);
        }

        // GET: Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Check based on role selection
                bool userExists = false;

                if (model.Role == "Customer")
                {
                    userExists = await _context.Users
                        .AnyAsync(u => u.Username == model.Username || u.Email == model.Email);
                }
                else if (model.Role == "Employee")
                {
                    userExists = await _context.Employees
                        .AnyAsync(e => e.Username == model.Username || e.Email == model.Email);
                }
                else if (model.Role == "Admin")
                {
                    userExists = await _context.Administrators
                        .AnyAsync(a => a.Username == model.Username || a.Email == model.Email);
                }

                if (userExists)
                {
                    ModelState.AddModelError("Username", "Username or Email already exists for this role");
                    return View(model);
                }

                // Hash the password
                var hashedPassword = _passwordHasher.HashPassword(model.Password);

                // Create user based on role
                if (model.Role == "Customer")
                {
                    var customer = new User
                    {
                        Username = model.Username,
                        Email = model.Email,
                        PasswordHash = hashedPassword,
                        FirstName = model.FirstName,
                        LastName = model.LastName
                    };
                    _context.Users.Add(customer);
                    await _context.SaveChangesAsync();

                    // Set session for auto-login
                    _httpContextAccessor.HttpContext.Session.SetInt32("UserId", customer.UserId);
                    _httpContextAccessor.HttpContext.Session.SetString("UserRole", "Customer");
                    _httpContextAccessor.HttpContext.Session.SetString("UserName", customer.Username);
                }
                else if (model.Role == "Employee")
                {
                    var employee = new Employee
                    {
                        Username = model.Username,
                        Email = model.Email,
                        PasswordHash = hashedPassword,
                        FirstName = model.FirstName,
                        LastName = model.LastName
                    };
                    _context.Employees.Add(employee);
                    await _context.SaveChangesAsync();

                    _httpContextAccessor.HttpContext.Session.SetInt32("UserId", employee.EmployeeId);
                    _httpContextAccessor.HttpContext.Session.SetString("UserRole", "Employee");
                    _httpContextAccessor.HttpContext.Session.SetString("UserName", employee.Username);
                }
                else if (model.Role == "Admin")
                {
                    var admin = new Administrator
                    {
                        Username = model.Username,
                        Email = model.Email,
                        PasswordHash = hashedPassword,
                        FirstName = model.FirstName,
                        LastName = model.LastName
                    };
                    _context.Administrators.Add(admin);
                    await _context.SaveChangesAsync();

                    _httpContextAccessor.HttpContext.Session.SetInt32("UserId", admin.AdminId);
                    _httpContextAccessor.HttpContext.Session.SetString("UserRole", "Admin");
                    _httpContextAccessor.HttpContext.Session.SetString("UserName", admin.Username);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"New {model.Role} registered: {model.Username}");

                // Redirect based on role
                if (model.Role == "Admin")
                    return RedirectToAction("ManageCustomers", "Admin");
                else if (model.Role == "Employee")
                    return RedirectToAction("Index", "BookingRequests");
                else
                    return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                return View(model);
            }
        

            var user = await _userService.CreateUserAsync(
                model.Username,
                model.Email,
                model.Password,
                model.FirstName,
                model.LastName
            );

            _httpContextAccessor.HttpContext.Session.SetInt32("UserId", user.UserId);
            _httpContextAccessor.HttpContext.Session.SetString("UserRole", "Customer");
            _httpContextAccessor.HttpContext.Session.SetString("UserName", user.Username);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            _httpContextAccessor.HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> HashExistingPasswords()
        {
            int count = 0;

            var employees = await _context.Employees.ToListAsync();
            foreach (var emp in employees)
            {
                if (emp.PasswordHash != null && emp.PasswordHash.Length < 64)
                {
                    emp.PasswordHash = _passwordHasher.HashPassword(emp.PasswordHash);
                    count++;
                }
            }

            var admins = await _context.Administrators.ToListAsync();
            foreach (var admin in admins)
            {
                if (admin.PasswordHash != null && admin.PasswordHash.Length < 64)
                {
                    admin.PasswordHash = _passwordHasher.HashPassword(admin.PasswordHash);
                    count++;
                }
            }

            var users = await _context.Users.ToListAsync();
            foreach (var user in users)
            {
                if (user.PasswordHash != null && user.PasswordHash.Length < 64)
                {
                    user.PasswordHash = _passwordHasher.HashPassword(user.PasswordHash);
                    count++;
                }
            }

            await _context.SaveChangesAsync();

            return Content($"Fixed {count} passwords. All passwords are now properly hashed.\n\n" +
                          $"You can now login with your existing passwords.\n" +
                          $"The hashed values are now:\n" +
                          $"'password123' → {_passwordHasher.HashPassword("password123")}\n" +
                          $"'admin123' → {_passwordHasher.HashPassword("admin123")}");
        }
    }
}
