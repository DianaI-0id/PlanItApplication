using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Diploma_Ishchenko.Services.DataCheckers
{
    public static class DataCheckerClass
    {
        public static bool IsValidPassword(string password) //проверяем ввод пароляы
        {
            var regex = new Regex(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{}|;:'"",.<>?/`~])[A-Za-z\d!@#$%^&*()_+\-=\[\]{}|;:'"",.<>?/`~]{8,}$");
            return regex.IsMatch(password);
        }
    }
}
