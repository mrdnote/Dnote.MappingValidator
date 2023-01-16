using System.Globalization;

using Dnote.MappingValidator.Library;

namespace Dnote.MappingValidator.Sample
{
    [ValidateMapping]
    public class FunctionMappers
    {
        [ValidateFunctionMapping]
        public static PersonViewModel PersonModelToPersonViewModel(PersonModel source)
        {
            return new PersonViewModel
            {
                Id = source.Id,
                FirstName = source.FirstName,
                LastName = source.LastName,
                FirstChildName = source.Children?.FirstOrDefault()?.FirstName,
                Street = source.Address?.Street,
                Number = source.Address?.Number,
                Pets = source.Pets.Select(p => new PetViewModel
                {
                    Name = p.Name
                })
            };
        }

        [ValidateFunctionMapping]
        public static PersonViewModel PersonModelToPersonViewModelExtraParameter(PersonModel source, CultureInfo? cultureInfo)
        {
            return new PersonViewModel
            {
                Id = source.Id,
                FirstName = source.FirstName?.ToUpper(cultureInfo),
                LastName = source.LastName,
                FirstChildName = source.Children?.FirstOrDefault()?.FirstName,
                Street = source.Address?.Street,
                Number = source.Address?.Number,
                Pets = source.Pets.Select(p => new PetViewModel
                {
                    Name = p.Name
                })
            };
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

        [ValidateFunctionMapping]
        public static PersonViewModel IncorrectPersonModelToPersonViewModel(PersonModel source)
        {
            return new PersonViewModel
            {
                Id = source.Id,
                FirstName = source.FirstName,
                LastName = source.LastName,
                Number = source.Address?.Number,
                Pets = source.Pets.Select(p => new PetViewModel
                {
                    Name = p.Name
                })
            };
        }

        [ValidateFunctionMapping]
        public static PersonViewModel IncorrectPetMappingPersonModelToPersonViewModel(PersonModel source)
        {
            return new PersonViewModel
            {
                Id = source.Id,
                FirstName = source.FirstName,
                LastName = source.LastName,
                FirstChildName = source.Children?.FirstOrDefault()?.FullName,
                Street = source.Address?.Street,
                Number = source.Address?.Number,
            };
        }

        [ValidateFunctionMapping]
        public static PersonViewModel IncorrectPetNameMappingPersonModelToPersonViewModel(PersonModel source)
        {
            return new PersonViewModel
            {
                Id = source.Id,
                FirstName = source.FirstName,
                LastName = source.LastName,
                FirstChildName = source.Children?.FirstOrDefault()?.FullName,
                Street = source.Address?.Street,
                Number = source.Address?.Number,
                Pets = source.Pets.Select(p => new PetViewModel
                {
                    // Forgot to map the Name property
                })
            };
        }
    }
}
