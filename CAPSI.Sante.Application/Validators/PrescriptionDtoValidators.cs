using CAPSI.Sante.Application.DTOs.Prescription;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Validators
{
    public class MedicamentPrescritDtoValidator : AbstractValidator<MedicamentPrescritDto>
    {
        public MedicamentPrescritDtoValidator()
        {
            RuleFor(dto => dto.NomMedicament)
                .NotEmpty().WithMessage("Le nom du médicament est requis.")
                .MaximumLength(100).WithMessage("Le nom du médicament ne peut pas dépasser 100 caractères.");

            RuleFor(dto => dto.Posologie)
                .NotEmpty().WithMessage("La posologie est requise.")
                .MaximumLength(255).WithMessage("La posologie ne peut pas dépasser 255 caractères.");
        }
    }

    public class CreatePrescriptionDtoValidator : AbstractValidator<CreatePrescriptionDto>
    {
        public CreatePrescriptionDtoValidator()
        {
            RuleFor(dto => dto.DossierId)
                .NotEmpty().WithMessage("L'ID du dossier est requis.");

            RuleFor(dto => dto.MedecinId)
                .NotEmpty().WithMessage("L'ID du médecin est requis.");

            RuleForEach(dto => dto.Medicaments)
                .SetValidator(new MedicamentPrescritDtoValidator());

            RuleFor(dto => dto.DateFin)
                .Must(BeAValidFutureDate).When(dto => dto.DateFin.HasValue)
                .WithMessage("La date de fin doit être future.");
        }

        private bool BeAValidFutureDate(DateTime? date)
        {
            return !date.HasValue || date.Value.Date >= DateTime.Now.Date;
        }
    }

    public class UpdatePrescriptionDtoValidator : AbstractValidator<UpdatePrescriptionDto>
    {
        public UpdatePrescriptionDtoValidator()
        {
            RuleFor(dto => dto.Id)
                .NotEmpty().WithMessage("L'ID de la prescription est requis.");

            RuleForEach(dto => dto.Medicaments)
                .SetValidator(new MedicamentPrescritDtoValidator());

            RuleFor(dto => dto.DateFin)
                .Must(BeAValidFutureDate).When(dto => dto.DateFin.HasValue)
                .WithMessage("La date de fin doit être future.");
        }

        private bool BeAValidFutureDate(DateTime? date)
        {
            return !date.HasValue || date.Value.Date >= DateTime.Now.Date;
        }
    }
}
