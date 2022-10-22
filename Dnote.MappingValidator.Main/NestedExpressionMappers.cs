using System.Linq.Expressions;

using Dnote.MappingValidator.Library;

using LinqKit;

namespace Dnote.MappingValidator.Sample
{
    [ValidateMapping]
    public static class NestedExpressionMappers
    {
        [ValidateMapping]
        public static Expression<Func<PersonModel, PersonViewModel>> PersonMapper
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
                    Pets = a.Pets.Select(p => PetMapper.Invoke(p))
                };
            }
        }

        [ValidateMapping]
        public static Expression<Func<PetModel, PetViewModel>> PetMapper
        {
            get
            {
                return p => new PetViewModel
                {
                    Name = p.Name
                };
            }
        }
    }
}