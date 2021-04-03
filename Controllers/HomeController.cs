using EmployeeManagement.Models;
using EmployeeManagement.Models.ViewModels;
using EmployeeManagement.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly ILogger<HomeController> logger;
        private readonly IDataProtector dataProtector;
        public HomeController(IEmployeeRepository employeeRepository, IWebHostEnvironment webHostEnvironment, ILogger<HomeController> logger, IDataProtectionProvider dataProtectionProvider, DataProtectionPurposeStrings dataProtectionPurposeStrings)
        {
            _employeeRepository = employeeRepository;
            this.webHostEnvironment = webHostEnvironment;
            this.logger = logger;
            dataProtector = dataProtectionProvider.CreateProtector(dataProtectionPurposeStrings.EmployeeIdRouteValue);
        }
        //public string Index() =>   _employeeRepository.GetEmployee(1).Name;
        [HttpGet]
        //[Route("")]
        //[Route("Home")]
        //[Route("[controller]/[action]")]
        [AllowAnonymous]
        public ViewResult Index()
        {
            var model = _employeeRepository.GetAllEmployee().Select(e=> 
            {
                e.EncryptedId = dataProtector.Protect(e.Id.ToString());
                return e;
            });
            return View(model);
        }

        public  ViewResult Details(string id)
        {
            //LogLevel 
            //logger.LogTrace("Trace Log"); //0
            //logger.LogDebug("Debug Log"); //1
            //logger.LogInformation("Information Log"); //2
            //logger.LogWarning("Warning Log"); //3
            //logger.LogError("Error Log"); //4
            //logger.LogCritical("Critical Log"); //5
            int employeeId=Convert.ToInt32(dataProtector.Unprotect(id));
            Employee employee = _employeeRepository.GetEmployee(employeeId);
            if (employee == null)
            {
                Response.StatusCode = 404;
                return View("EmployeeNotFound", employeeId);
            }
            employee.EncryptedId = id;
            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                Employee = employee,
                //Employee = _employeeRepository.GetEmployee(id),
                PageTitle = "Employee Details"
            };
            //Employee model = _employeeRepository.GetEmployee(1);
            //ViewBag.PageTitle = "Employee Details";
            //ViewBag.Employee = model;
            return View(homeDetailsViewModel);
        }
        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(EmployeeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                //multiple file uploads
                //string uniqueFileName = null;
                //if (model.Photos!=null && model.Photos.Count>0)
                //{
                //    foreach (IFormFile photo in model.Photos)
                //    {
                //        string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");
                //        uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                //        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                //        photo.CopyTo(new FileStream(filePath, FileMode.Create));
                //    }

                //}
                Employee NewEmployee = new Employee
                {
                    Name = model.Name,
                    Department = model.Department,
                    Email = model.Email,
                    PhotoPath =ProcessUploadedFile(model)
                };
                _employeeRepository.Add(NewEmployee);
                return RedirectToAction("details", new { id = NewEmployee.Id });
            }
            return View();
        }
        public ViewResult Edit(string id)
        {
            int employeeId = Convert.ToInt32(dataProtector.Unprotect(id));
            Employee employee = _employeeRepository.GetEmployee(employeeId);
            EmployeeEditViewModel employeeEditViewModel = new EmployeeEditViewModel
            {
                Id = employee.Id,
                EncrypedId=dataProtector.Protect(employee.Id.ToString()),
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                ExistingPhotoPath = employee.PhotoPath
            };
            return View(employeeEditViewModel);
        }
        [HttpPost]
        public IActionResult Edit(EmployeeEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                Employee employee = _employeeRepository.GetEmployee(model.Id);
                employee.Name = model.Name;
                employee.Email = model.Email;
                employee.Department = model.Department;
                employee.PhotoPath = model.ExistingPhotoPath;
                if (model.Photos!=null)
                {
                    if (model.ExistingPhotoPath!=null)
                    {
                        string filePath = Path.Combine(webHostEnvironment.WebRootPath, "images", model.ExistingPhotoPath);
                        System.IO.File.Delete(filePath);
                    }
                    employee.PhotoPath= ProcessUploadedFile(model);
                }
                
                _employeeRepository.Update(employee);
                return RedirectToAction("index");
            }
            return View();
        }

        private string ProcessUploadedFile(EmployeeCreateViewModel model)
        {
            string uniqueFileName = null;
            if (model.Photos != null && model.Photos.Count > 0)
            {
                foreach (IFormFile photo in model.Photos)
                {
                    string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream= new FileStream(filePath, FileMode.Create))
                    {
                        photo.CopyTo(fileStream);
                    }
                    
                }

            }

            //string uniqueFileName = null;
            //if (model.Photos != null)
            //{
            //    string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");
            //    uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
            //    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            //    photo.CopyTo(new FileStream(filePath, FileMode.Create));
            //}
            return uniqueFileName;
        }
    }
}
