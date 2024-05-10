using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace Utils
{
    public class FieldValidator
    {
        private static string requiredMessage = "Please enter an";
        private static string validMessage = "Please enter a valid";
        public static string requiredFieldMessage(string text, string fieldName)
        {
            if (String.IsNullOrEmpty(text))
                return $"{requiredMessage} {fieldName}.";
            return string.Empty;
        }

        public static string emailErrMessage(string email)
        {
            if (String.IsNullOrEmpty(email))
                return $"{requiredMessage} email address.";
            else if (email.IndexOf("@") < 0 || email.IndexOf(".") < 0 || !(new EmailAddressAttribute().IsValid(email)) || email.Contains(" "))
                return $"{validMessage} email address.";
            return string.Empty;
        }

        public static string mobileErrMessage(string mobileNo)
        {
            if (String.IsNullOrEmpty(mobileNo))
                return $"{requiredMessage} Mobile number.";
            else if(mobileNo.Length < 10 || !double.TryParse(mobileNo, out double mobile) || !Regex.IsMatch(mobileNo, @"^\d+$"))
                return $"{validMessage} Mobile number";
            return string.Empty;
        }

        public static string passwordPinErrorMessage(string password)
        {
            if (String.IsNullOrEmpty(password))
                return $"{requiredMessage} password.";
            else if (password.Length < 6 || password.Length > 6)
                return "Please provide 6 digit password only.";
            else
            {
                bool hasSymbol = false;
                bool hasUpper = false;
                bool hasLower = false;
                string errorMessage = "";
                string specialChar = @"\|!#$%&/()=?»«@£§€{}.-;'<>_,";
                foreach (char c in password)
                {
                    if (specialChar.IndexOf(c) > -1)
                        hasSymbol = true;
                    else if (char.IsUpper(c))
                        hasUpper = true;
                    else if (char.IsLower(c))
                        hasLower = true;
                }

                if (hasSymbol == true)
                {
                    errorMessage += "Password must contain 6 digit numbers only.";
                    return errorMessage;
                }

                if (hasLower == true)
                {
                    errorMessage += "Password must contain 6 digit numbers only.";
                    return errorMessage;
                }

                if (hasUpper == true)
                {
                    errorMessage += "Password must contain 6 digit numbers only.";
                    return errorMessage;
                }
            }
            return string.Empty;
        }

        public static string passwordErrorMessage(string password)
        {
            if (String.IsNullOrEmpty(password))
                return $"{requiredMessage} password.";
            else if (password.Length < 6 || password.Length > 100)
                return "Password must be at least 6 and at max 100 characters long.";
            else {
                bool hasDigit = false;
                bool hasSymbol = false;
                bool hasUpper = false;
                bool hasLower = false;
                string errorMessage = "";
                string specialChar = @"\|!#$%&/()=?»«@£§€{}.-;'<>_,";
                foreach (char c in password)
                {
                    if (specialChar.IndexOf(c) > -1)
                        hasSymbol = true;
                    else if (char.IsNumber(c))
                        hasDigit = true;
                    else if (char.IsUpper(c))
                        hasUpper = true;
                    else if (char.IsLower(c))
                        hasLower = true;
                }

                if (!hasSymbol || !hasDigit || !hasLower || !hasUpper) {
                    errorMessage += "Passwords must have at least one:";
                    if (!hasSymbol)
                        errorMessage += "\r\n non alphanumeric character";
                    if (!hasDigit)
                        errorMessage += "\r\n digit ('0'-'9')";
                    if (!hasLower)
                        errorMessage += "\r\n lowercase ('a'-'z')";
                    if(!hasUpper)
                        errorMessage += "\r\n uppercase ('A'-'Z')";
                    return errorMessage;
                }
            }
            return string.Empty;
        }

        public static string confirmPasswordErrMessage(string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;
            else if (string.IsNullOrEmpty(confirmPassword))
                return $"{requiredMessage} confirm password";
            if (!password.Equals(confirmPassword))
                return "The password and confirmation password do not match.";
            return string.Empty;
        }

        public static string websiteErrMessage(string website)
        {
            if (string.IsNullOrEmpty(website))
                return string.Empty;
            if(!website.Contains("www."))
                return $"{validMessage} Website must start with 'wwww.'";
            return string.Empty;
        }
    }
}
