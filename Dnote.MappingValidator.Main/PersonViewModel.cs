namespace Dnote.MappingValidator.Sample
{
    public class PersonViewModel
    {
#pragma warning disable CS0414,IDE0051,IDE0044
        private string _somePrivateProperty = "not used";
#pragma warning restore IDE0044,IDE0051,CS0414

#pragma warning disable CA1822 // Mark members as static
        public string? SomeWriteOnlyProperty
#pragma warning restore CA1822 // Mark members as static
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
