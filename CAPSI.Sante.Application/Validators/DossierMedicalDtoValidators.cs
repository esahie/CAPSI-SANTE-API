using CAPSI.Sante.Application.DTOs.DossierMedical;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Validators
{
    public class CreateDossierMedicalDtoValidator : AbstractValidator<CreateDossierMedicalDto>
    {
        public CreateDossierMedicalDtoValidator()
        {
            RuleFor(dto => dto.PatientId)
                .NotEmpty().WithMessage("L'ID du patient est requis.");
        }
    }

    public class UpdateDossierMedicalDtoValidator : AbstractValidator<UpdateDossierMedicalDto>
    {
        public UpdateDossierMedicalDtoValidator()
        {
            RuleFor(dto => dto.Id)
                .NotEmpty().WithMessage("L'ID du dossier est requis.");

            RuleFor(dto => dto.PatientId)
                .NotEmpty().WithMessage("L'ID du patient est requis.");
        }
    }
}
