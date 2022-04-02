namespace Dnote.MappingValidator.Main
{
    public class AddressModel
    {
        public string Street { get; set; } = null!;

        public int Number { get; set; }
        
        public PersonModel? Owner { get; set; }
    }
}
