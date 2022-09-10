namespace Dnote.MappingValidator.Sample
{
    public class PersonViewModel
    {
        private string _somePrivateProperty = "not used";

        public string SomeWriteOnlyProperty
        {
            set { }
        }

        public int Id { get; set; }

        public string? FirstName { get; set; }

        public string LastName { get; set; } = null!;

        public string FullName => FirstName + LastName;

        public string? FirstChildName { get; set; }

        public string? Street { get; set; }

        public int? Number { get; set; }

        public IEnumerable<PetViewModel>? Pets { get; set; }
    }
}
