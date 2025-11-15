using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UMS.Core.Dto
{
    public class UserPasswordResetRequest
    {
     
        public string OldPassword { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [MinLength(8)]
        [MaxLength(15)]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).\S{7,14}$", ErrorMessage = "Password must be in between (8-15) range, contain at least 1 number,1 special character, mixture of uppercase and lowercase letters.")]
        public string Password { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "ConfirmPassword is required")]
        [MinLength(8)]
        [MaxLength(15)]
        [DataType(DataType.Password)]
        [Compare("Password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).\S{7,14}$", ErrorMessage = "Password must be in between (8-15) range, contain at least 1 number,1 special character, mixture of uppercase and lowercase letters.")]
        public string ConfirmPassword { get; set; }

        public string Username { get; set; }

        public bool IsAdmin { get; set; }

    }
}
