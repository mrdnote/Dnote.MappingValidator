using System.Globalization;

using Dnote.MappingValidator.Library;

namespace Dnote.MappingValidator.Sample
{
    [ValidateMapping]
    public class ProceduralMappers
    {
        [ValidateProcedureMapping()]
        public static void PersonModelToPersonViewModel(PersonModel source, PersonViewModel destination)
        {
            destination.Id = source.Id;
            destination.FirstName = source.FirstName;
            destination.LastName = source.LastName;
            destination.FirstChildName = source.Children?.FirstOrDefault()?.FirstName;
            destination.Street = source.Address?.Street;
            destination.Number = source.Address?.Number;
            destination.Pets = source.Pets.Select(p => new PetViewModel
            {
                Name = p.Name
            });
        }

        [ValidateProcedureMapping()]
        public static void PersonModelToPersonViewModelExtraParameter(PersonModel source, PersonViewModel destination, CultureInfo? cultureInfo)
        {
            destination.Id = source.Id;
            destination.FirstName = source.FirstName?.ToUpper(cultureInfo);
            destination.LastName = source.LastName;
            destination.FirstChildName = source.Children?.FirstOrDefault()?.FirstName;
            destination.Street = source.Address?.Street;
            destination.Number = source.Address?.Number;
            destination.Pets = source.Pets.Select(p => new PetViewModel
            {
                Name = p.Name
            });
        }

        [ValidateProcedureMapping(false, nameof(PersonViewModel.LastName))]
        public static void CorrectMappingPersonModelToPersonViewModelExcludingLastName(PersonModel source, PersonViewModel destination)
        {
            destination.Id = source.Id;
            destination.FirstName = source.FirstName;
            destination.Street = source.Address?.Street;
            destination.Number = source.Address?.Number;
            destination.FirstChildName = source.Children?.FirstOrDefault()?.FullName;
            destination.Pets = source.Pets.Select(p => new PetViewModel
            {
                Name = p.Name
            });
        }

        [ValidateProcedureMapping(false, $"{nameof(PersonViewModel.Pets)}.{nameof(PetViewModel.Name)}")]
        public static void CorrectMappingPersonModelToPersonViewModelExcludingPetName(PersonModel source, PersonViewModel destination)
        {
            destination.Id = source.Id;
            destination.FirstName = source.FirstName;
            destination.LastName = source.LastName;
            destination.Street = source.Address?.Street;
            destination.Number = source.Address?.Number;
            destination.FirstChildName = source.Children?.FirstOrDefault()?.FullName;
            destination.Pets = source.Pets.Select(p => new PetViewModel
            {
                // Deliberately skip the pet's name here
            });
        }

        [ValidateProcedureMapping]
        public static void IncorrectPersonModelToPersonViewModel(PersonModel source, PersonViewModel destination)
        {
            destination.Id = source.Id;
            destination.FirstName = source.FirstName;
            destination.LastName = source.LastName;
            destination.Number = source.Address?.Number;
            destination.Pets = source.Pets.Select(p => new PetViewModel
            {
                Name = p.Name
            });
        }

        [ValidateProcedureMapping]
        public static void IncorrectPetMappingPersonModelToPersonViewModel(PersonModel source, PersonViewModel destination)
        {
            destination.Id = source.Id;
            destination.FirstName = source.FirstName;
            destination.LastName = source.LastName;
            destination.FirstChildName = source.Children?.FirstOrDefault()?.FullName;
            destination.Street = source.Address?.Street;
            destination.Number = source.Address?.Number;
        }

        [ValidateProcedureMapping]
        public static void IncorrectPetNameMappingPersonModelToPersonViewModel(PersonModel source, PersonViewModel destination)
        {
            destination.Id = source.Id;
            destination.FirstName = source.FirstName;
            destination.LastName = source.LastName;
            destination.FirstChildName = source.Children?.FirstOrDefault()?.FullName;
            destination.Street = source.Address?.Street;
            destination.Number = source.Address?.Number;
            destination.Pets = source.Pets.Select(p => new PetViewModel
            {
                // Forgot to map the Name property
            });
        }
    }
}
