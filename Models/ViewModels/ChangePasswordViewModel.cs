using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Models.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name ="Current password")]
        public string CurrentPassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name ="New password")]
        public string NewPassword { get; set; }
        [DataType(DataType.Password)]
        [Display(Name ="Confirm password")]
        [Compare("NewPassword",ErrorMessage = "New password and confirm password does not match")]
        public string ComfirmPassword { get; set; }
    }
}
