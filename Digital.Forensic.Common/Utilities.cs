using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Digital.Forensic.Common
{
   public static class Utilities
    {
        public static string TrimDate(string x)
        {
            string date = x;
            string strippedDate = date.Remove(24);
            string[] brokenDownDate = strippedDate.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string month = brokenDownDate[1];
            string dayNumber = brokenDownDate[2];
            string year = brokenDownDate[3];
            string time = brokenDownDate[4];
            switch (month)
            {
                case "Jan":
                    month = "01";
                    break;
                case "Feb":
                    month = "02";
                    break;
                case "Mar":
                    month = "03";
                    break;
                case "Apr":
                    month = "04";
                    break;
                case "May":
                    month = "05";
                    break;
                case "Jun":
                    month = "06";
                    break;
                case "Jul":
                    month = "07";
                    break;
                case "Aug":
                    month = "08";
                    break;
                case "Sep":
                    month = "09";
                    break;
                case "Oct":
                    month = "10";
                    break;
                case "Nov":
                    month = "11";
                    break;
                case "Dec":
                    month = "12";
                    break;
            }
            string trimmedDate = year + "/" + month + "/" + dayNumber;
            return trimmedDate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileExt"></param>
        /// <returns></returns>
        public static bool IsAllowedImageExtension(string fileExt)
        {
            bool isValid = false;

            string[] allowExt = { ".jpg", ".png", ".jpeg", ".bmp" };//Add to the array if desired

            for (int i = 0; i <= allowExt.Length - 1; i++)
            {
                if (fileExt == allowExt[i])
                {
                    isValid = true;
                    break;
                }
            }
            return isValid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isUrlSecure"></param>
        /// <returns></returns>
        public static string GetApplicationLocation(bool isUrlSecure = false)
        {
            Uri uri = HttpContext.Current.Request.Url;
            string app = HttpContext.Current.Request.ApplicationPath;
            if (!app.EndsWith("/"))
            {
                app += "/";
            }

            if (isUrlSecure)
            {
                return String.Format("https://{0}:{1}{2}", uri.Host, uri.Port, app);
            }
            else
            {
                return String.Format("http://{0}:{1}{2}", uri.Host, uri.Port, app);
            }


        }

        /// <summary>
        /// Verifies that a string is in valid e-mail format
        /// </summary>
        /// <param name="email">Email to verify</param>
        /// <returns>true if the string is a valid e-mail address and false if it's not</returns>
        public static bool IsValidEmail(string email)
        {
            if (String.IsNullOrEmpty(email))
                return false;

            email = email.Trim();
            var result = Regex.IsMatch(email, "^(?:[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+\\.)*[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+@(?:(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9\\-](?!\\.)){0,61}[a-zA-Z0-9]?\\.)+[a-zA-Z0-9](?:[a-zA-Z0-9\\-](?!$)){0,61}[a-zA-Z0-9]?)|(?:\\[(?:(?:[01]?\\d{1,2}|2[0-4]\\d|25[0-5])\\.){3}(?:[01]?\\d{1,2}|2[0-4]\\d|25[0-5])\\]))$", RegexOptions.IgnoreCase);
            return result;
        }

        /// <summary>
        /// Generate random digit code
        /// </summary>
        /// <param name="length">Length</param>
        /// <returns>Result string</returns>
        public static string GenerateRandomDigitCode(int length)
        {
            var random = new Random();
            string str = string.Empty;
            for (int i = 0; i < length; i++)
                str = String.Concat(str, random.Next(10).ToString());
            return str;
        }

        public static string GenerateKey(int length)
        {
            var random = new Random();
            const string chars = "#ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(x => x[random.Next(x.Length)]).ToArray());
        }

        /// <summary>
        /// Returns an random interger number within a specified rage
        /// </summary>
        /// <param name="min">Minimum number</param>
        /// <param name="max">Maximum number</param>
        /// <returns>Result</returns>
        public static int GenerateRandomInteger(int min = 0, int max = int.MaxValue)
        {
            var randomNumberBuffer = new byte[10];
            new RNGCryptoServiceProvider().GetBytes(randomNumberBuffer);
            return new Random(BitConverter.ToInt32(randomNumberBuffer, 0)).Next(min, max);
        }

        /// <summary>
        /// Ensure that a string doesn't exceed maximum allowed length
        /// </summary>
        /// <param name="str">Input string</param>
        /// <param name="maxLength">Maximum length</param>
        /// <param name="postfix">A string to add to the end if the original string was shorten</param>
        /// <returns>Input string if its lengh is OK; otherwise, truncated input string</returns>
        public static string EnsureMaximumLength(string str, int maxLength, string postfix = null)
        {
            if (String.IsNullOrEmpty(str))
                return str;

            if (str.Length > maxLength)
            {
                var result = str.Substring(0, maxLength);
                if (!String.IsNullOrEmpty(postfix))
                {
                    result += postfix;
                }
                return result;
            }

            return str;
        }

        /// <summary>
        /// Ensures that a string only contains numeric values
        /// </summary>
        /// <param name="str">Input string</param>
        /// <returns>Input string with only numeric values, empty string if input is null/empty</returns>
        public static string EnsureNumericOnly(string str)
        {
            if (String.IsNullOrEmpty(str))
                return string.Empty;

            var result = new StringBuilder();
            foreach (char c in str)
            {
                if (Char.IsDigit(c))
                    result.Append(c);
            }
            return result.ToString();
        }

        /// <summary>
        /// Get substring of specified number of characters on the right.
        /// </summary>
        public static string Right(this string value, int length)
        {
            if (String.IsNullOrEmpty(value)) return string.Empty;

            return value.Length <= length ? value : value.Substring(value.Length - length);
        }

        /// <summary>
        /// Get substring of specified number of characters on the left.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Left(this string value, int length)
        {
            if (String.IsNullOrEmpty(value)) return string.Empty;

            return value.Length <= length ? value : value.Substring(0, length);
        }

        /// <summary>
        /// Format any decimal value to a specified decimal places
        /// </summary>
        /// <param name="value">Decimal value to format</param>
        /// <param name="decimalPlace">No of decimal Places</param>
        /// <returns>Returns a string in a formatted form</returns>
        /// <summary>
        public static string FormatMoney(this decimal value, int decimalPlace)
        {
            //if (value == 0) return string.Empty;
            string zeroAmount = "0";
            string concatDecimalPlacesForZeroAmount = string.Empty;
            string decimalPlaceConcatResult = string.Empty;
            for (int i = 1; i <= decimalPlace; i++)
            {
                decimalPlaceConcatResult += "0";
                concatDecimalPlacesForZeroAmount += "0";
            }
            if (value == 0)
            {
                return $"{zeroAmount}.{concatDecimalPlacesForZeroAmount}";
            }
            return string.Format("{0:#,##0." + decimalPlaceConcatResult + "}", value);
        }
    }
}
