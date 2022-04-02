namespace Dnote.MappingValidator.Main
{
    public class PersonModel
    {
        private string _somePropertyThatIsNotUsed = "notused";

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
