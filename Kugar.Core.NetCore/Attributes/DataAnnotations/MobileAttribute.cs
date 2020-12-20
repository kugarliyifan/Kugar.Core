using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Attributes.DataAnnotations
{

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
    public class MobileAttribute : ValidationAttribute
    {
        public MobileAttribute()
            : base("The {0} field is not a valid phone number.")
        {
        }


        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var v = value.ToStringEx();

            if (string.IsNullOrWhiteSpace(v) || !v.IsMatchPhoneNumber())
            {
                return new ValidationResult(ErrorMessageString);
            }
            else
            {
                return ValidationResult.Success;
            }
        }
    }
}
