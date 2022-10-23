namespace Dnote.MappingValidator.Sample
{
    public class PersonModel
    {
#pragma warning disable CS0414,IDE0051,IDE0044
        private string _somePropertyThatIsNotUsed = "notused";
#pragma warning restore IDE0044,IDE0051,CS0414

        public string SomeWriteOnlyProperty
        {
            set { }
        }

        public string? NotUsed { get; set; }

        public int Id { get; set; }

        public string? FirstName { get; set; }

        public string LastName { get; set; } = null!;

        public string FullName => FirstName + LastName;

        public AddressModel? Address { get; set; }

        public IEnumerable<PersonModel>? Children { get; set; }

        public IEnumerable<PetModel> Pets { get; set; } = null!;

        public Gender? Gender { get; set; }
    }
}
