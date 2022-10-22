using System.Linq.Expressions;

using Dnote.MappingValidator.Library;

namespace Dnote.MappingValidator.Sample
{
    [ValidateMapping]
    public class ProceduralMappers
    {
        [ValidateMapping]
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

        [ValidateMapping(false, nameof(PersonViewModel.LastName))]
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

        [ValidateMapping(false, $"{nameof(PersonViewModel.Pets)}.{nameof(PetViewModel.Name)}")]
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

        [ValidateMapping]
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

        [ValidateMapping]
        public static void IncorrectPetMappingPersonModelToPersonViewModel(PersonModel source, PersonViewModel destination)
        {
            destination.Id = source.Id;
            destination.FirstName = source.FirstName;
            destination.LastName = source.LastName;
            destination.FirstChildName = source.Children?.FirstOrDefault()?.FullName;
            destination.Street = source.Address?.Street;
            destination.Number = source.Address?.Number;
        }

        [ValidateMapping]
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
