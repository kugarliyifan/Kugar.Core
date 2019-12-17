using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Kugar.Core.DataAnnotations
{
    public class GreaterThanAttribute : ValidationAttribute
    {
        public GreaterThanAttribute(double checkNum)
            : base("请输入正确的{0},参数必须大于{1}")
        {
            CheckNum = (decimal)checkNum;
        }

        public GreaterThanAttribute(int checkNum)
            : base("请输入正确的{0},参数必须大于{1}")
        {
            CheckNum = (decimal)checkNum;
        }

        public decimal CheckNum { set; get; }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(CultureInfo.CurrentUICulture, ErrorMessage, name, CheckNum);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            try
            {
                var v = (decimal)value;

                if (v <= CheckNum)
                {
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
                }

                return ValidationResult.Success;
            }
            catch (Exception)
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
        }
    }


    public class GreaterOrEqualThanAttribute : ValidationAttribute 
    {
        public GreaterOrEqualThanAttribute(double checkNum):base("请输入正确的{0},参数必须大于或等于{1}")
        {
            CheckNum = (decimal)checkNum;
        }

        public GreaterOrEqualThanAttribute(int checkNum)
            : base("请输入正确的{0},参数必须大于或等于{1}")
        {
            CheckNum = (decimal)checkNum;
        }

        public decimal CheckNum { set; get; }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(CultureInfo.CurrentUICulture, ErrorMessage, name, CheckNum);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            try
            {
                var v = (decimal)value;

                if (v < CheckNum)
                {
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
                }

                return ValidationResult.Success;
            }
            catch (Exception)
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
        }
    }





}
