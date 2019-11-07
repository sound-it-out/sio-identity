using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace SIO.Identity.Tests
{
    public static class ControllerExtensions
    {
        public static void ValidateRequest(this Controller controller, object model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateObject(model, context, results, true))
                foreach (var error in results)
                {
                    if (!error.MemberNames.Any())
                        controller.ModelState.AddModelError("", error.ErrorMessage);
                    else
                        foreach (var member in error.MemberNames)
                            controller.ModelState.AddModelError(member, error.ErrorMessage);
                }
        }
    }
}
