using System.Linq.Expressions;

using Dnote.MappingValidator.Library;

namespace Dnote.MappingValidator.Sample
{
    [ValidateMapping]
    public class ExpressionMappers
    {
        [ValidateMapping]
        public static Expression<Func<PersonModel, PersonViewModel>> PersonModelToPersonViewModel
        {
            get
            {
                return a => new PersonViewModel
                {
                    Id = a.Id,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    Street = a.Address != null ? a.Address.Street : null,
                    Number = a.Address != null ? a.Address.Number : null,
                    FirstChildName = a.Children != null && a.Children.Any() ? a.Children.First().FullName : null,
                    Pets = a.Pets.Select(p => new PetViewModel
                    {
                        Name = p.Name
                    })
                };
            }
        }

        [ValidateMapping(false, nameof(PersonViewModel.LastName))]
        public static Expression<Func<PersonModel, PersonViewModel>> CorrectMappingPersonModelToPersonViewModelExcludingLastName
        {
            get
            {
                return a => new PersonViewModel
                {
                    Id = a.Id,
                    FirstName = a.FirstName,
                    Street = a.Address != null ? a.Address.Street : null,
                    Number = a.Address != null ? a.Address.Number : null,
                    FirstChildName = a.Children != null && a.Children.Any() ? a.Children.First().FullName : null,
                    Pets = a.Pets.Select(p => new PetViewModel
                    {
                        Name = p.Name
                    })
                };
            }
        }

        [ValidateMapping(false, $"{nameof(PersonViewModel.Pets)}.{nameof(PetViewModel.Name)}")]
        public static Expression<Func<PersonModel, PersonViewModel>> CorrectMappingPersonModelToPersonViewModelExcludingPetName
        {
            get
            {
                return a => new PersonViewModel
                {
                    Id = a.Id,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    Street = a.Address != null ? a.Address.Street : null,
                    Number = a.Address != null ? a.Address.Number : null,
                    FirstChildName = a.Children != null && a.Children.Any() ? a.Children.First().FullName : null,
                    Pets = a.Pets.Select(p => new PetViewModel
                    {
                        // Deliberately skip the pet's name here
                    })
                };
            }
        }

        [ValidateMapping]
        public static Expression<Func<PersonModel, PersonViewModel>> IncorrectPersonModelToPersonViewModel
        {
            get
            {
                return a => new PersonViewModel
                {
                    Id = a.Id,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    Number = a.Address != null ? a.Address.Number : null,
                    Pets = a.Pets.Select(p => new PetViewModel
                    {
                        Name = p.Name
                    })
                };
            }
        }

        [ValidateMapping]
        public static Expression<Func<PersonModel, PersonViewModel>> IncorrectPetMappingPersonModelToPersonViewModel
        {
            get
            {
                return a => new PersonViewModel
                {
                    Id = a.Id,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    FirstChildName = a.Children != null && a.Children.Any() ? a.Children.First().FullName : null,
                    Street = a.Address != null ? a.Address.Street : null,
                    Number = a.Address != null ? a.Address.Number : null
                };
            }
        }

        [ValidateMapping]
        public static Expression<Func<PersonModel, PersonViewModel>> IncorrectPetNameMappingPersonModelToPersonViewModel
        {
            get
            {
                return a => new PersonViewModel
                {
                    Id = a.Id,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    FirstChildName = a.Children != null && a.Children.Any() ? a.Children.First().FullName : null,
                    Street = a.Address != null ? a.Address.Street : null,
                    Number = a.Address != null ? a.Address.Number : null,
                    Pets = a.Pets.Select(p => new PetViewModel
                    {
                        // Forgot to map the Name property
                    })
                };
            }
        }
    }
}