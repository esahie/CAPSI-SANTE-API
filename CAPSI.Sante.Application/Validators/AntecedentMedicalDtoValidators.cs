using CAPSI.Sante.Application.DTOs.AntecedentMedical;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Validators
{
    public class CreateAntecedentMedicalDtoValidator : AbstractValidator<CreateAntecedentMedicalDto>
    {
        public CreateAntecedentMedicalDtoValidator()
        {
            RuleFor(dto => dto.DossierId)
                .NotEmpty().WithMessage("L'ID du dossier est requis.");

            RuleFor(dto => dto.Type)
                .NotEmpty().WithMessage("Le type d'antécédent est requis.")
                .MaximumLength(100).WithMessage("Le type d'antécédent ne peut pas dépasser 100 caractères.");

            RuleFor(dto => dto.Description)
                .NotEmpty().WithMessage("La description est requise.");
        }
    }

    public class UpdateAntecedentMedicalDtoValidator : AbstractValidator<UpdateAntecedentMedicalDto>
    {
        public UpdateAntecedentMedicalDtoValidator()
        {
            RuleFor(dto => dto.Id)
                .NotEmpty().WithMessage("L'ID de l'antécédent est requis.");

            RuleFor(dto => dto.Type)
                .NotEmpty().WithMessage("Le type d'antécédent est requis.")
                .MaximumLength(100).WithMessage("Le type d'antécédent ne peut pas dépasser 100 caractères.");

            RuleFor(dto => dto.Description)
                .NotEmpty().WithMessage("La description est requise.");
        }
    }
}
