using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimBankSite.Models
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }
        public ApplicationUser User { get; set; }
        public string RoleName { get; set; }
    }

    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    public class SetPasswordViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "Значение {0} должно содержать символов не менее: {2}.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение нового пароля")]
        [Compare("NewPassword", ErrorMessage = "Новый пароль и его подтверждение не совпадают.")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Значение {0} должно содержать символов не менее: {2}.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение нового пароля")]
        [Compare("NewPassword", ErrorMessage = "Новый пароль и его подтверждение не совпадают.")]
        public string ConfirmPassword { get; set; }
    }

    public class AddPhoneNumberViewModel
    {
        [Required]
        [Phone]
        [Display(Name = "Номер телефона")]
        public string Number { get; set; }
    }

    public class VerifyPhoneNumberViewModel
    {
        [Required]
        [Display(Name = "Код")]
        public string Code { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Номер телефона")]
        public string PhoneNumber { get; set; }
    }

    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    }

    //Наши классы

    public class MoneyAddOnBalance
    {
        [Required]
        [Display(Name = "Количество денег для занесения")]
        [DataType(DataType.Currency)]
        public string Money { get; set; }
    }

    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Сумма пополнения")]
        [DataType(DataType.Currency)]
        public string Sum { get; set; }

        // navigation property navernoe
        [Required]
        [ForeignKey("AppUser")]
        public string UserId
        {
            get
            {
                if(System.Web.HttpContext.Current != null)
                {
                    return System.Web.HttpContext.Current.User.Identity.GetUserId();
                }

                return null;
            }
            set
            {
                if(UserId == null && System.Web.HttpContext.Current != null)
                {
                    UserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                }    
            }
        }

        public virtual ApplicationUser AppUser { get; set; }
    }


    /// <summary>
    /// Класс транзакции
    /// </summary>
    //public class Transaction
    //{
    //    public int Id { get; set; }

    //    // navigation property navernoe
    //    //[Required]
    //    [ForeignKey("AppUser")]
    //   /* public string UserID
    //    {
    //        get; set;
    //        //get
    //        //{
    //        //    return System.Web.HttpContext.Current.User.Identity.GetUserId();
    //        //}
    //        //set
    //        //{
    //        //    UserID = "1";
    //        //}
    //    }*/

    //    [Required]
    //    [Display(Name = "Сумма пополнения")]
    //    [DataType(DataType.Currency)]
    //    public string Sum { get; set; }

    //    public virtual ApplicationUser AppUser { get; set; }
    //}
}