using System;
using System.Text;

namespace SterlingDB.Test.Helpers
{
    public static class ExceptionHelper
    {
        public static string AsExceptionString(this Exception ex)
        {
            var exception = ex;
            var sb = new StringBuilder();

            while (exception != null)
            {
                sb.Append(ex);
                if (exception.InnerException == null) break;
                sb.Append(" - with inner exception: ");
                exception = exception.InnerException;
            }

            return sb.ToString();
        }
    }
}