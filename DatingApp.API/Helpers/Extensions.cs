using System;
using Microsoft.AspNetCore.Http;

namespace DatingApp.API.Helpers
{
    public static class Extensions
    {
        // Adding headers for error handling and adding a CORS error header
        public static void AddApplicationError(this HttpResponse response, string message)
        {
            // In the event of an exception, "Application-Error" header is added with the error "message" as value
            response.Headers.Add("Application-Error", message);
            // These other two allows the top header to be displayed
            response.Headers.Add("Access-Control-Expose-Headers", "Application-Error");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        // Using "this" because we are adding an extension method to an existing class
        public static int CalculateAge(this DateTime theDateTime)
        {
            var age = DateTime.Today.Year - theDateTime.Year;
            if (theDateTime.AddYears(age) > DateTime.Today)
                age--;

            return age;
        }
    }
}