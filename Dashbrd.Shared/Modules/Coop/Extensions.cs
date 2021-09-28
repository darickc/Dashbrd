using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;

namespace Dashbrd.Shared.Modules.Coop
{
    public static class Extensions
    {
        public static Result<int> ToInt(this string text)
        {
            if (int.TryParse(text, out var value))
            {
                return value;
            }

            return Result.Failure<int>("Could not parse.");
        }

        public static Result<string> Decode(this byte[] data)
        {
            var payload = data?.Any() == true
                ? Encoding.UTF8.GetString(data, 0, data.Length)
                : null;
            return payload;
        }

        public static Result<bool> ToBool(this string text)
        {
            if (text == "on")
                return true;
            if (text == "off")
                return false;
            return Result.Failure<bool>("Value not valid.");
        }
    }
}