using CAPSI.Sante.Application.DTOs.Auth;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Validators
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("L'email est requis")
                .EmailAddress().WithMessage("Format d'email invalide");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Le mot de passe est requis")
                .MinimumLength(8).WithMessage("Le mot de passe doit contenir au moins 8 caractères")
                .Matches("[A-Z]").WithMessage("Le mot de passe doit contenir au moins une majuscule")
                .Matches("[0-9]").WithMessage("Le mot de passe doit contenir au moins un chiffre")
                .Matches("[^a-zA-Z0-9]").WithMessage("Le mot de passe doit contenir au moins un caractère spécial");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("Les mots de passe ne correspondent pas");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Le rôle est requis")
                .Must(role => new[] { "Admin", "Medecin", "Patient" }.Contains(role))
                .WithMessage("Rôle invalide");
        }
    }
}
