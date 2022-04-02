using System.Reflection;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dnote.MappingValidator.Main;
using System.Collections.Generic;

namespace Dnote.MappingValidator.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Person_is_mapped()
        {
            var person = new PersonModel
            {
                FirstName = "Dave",
                LastName = "de Jong",
                Address = new AddressModel
                {
                    Street = "Kalverstraat",
                    Number = 1
                },
                Pets = new List<PetModel>
                {
                    new PetModel
                    {
                        Name = "Boris"
                    }
                }
            };

            var mappingFunc = PersonMapper.PersonModelToPersonViewModel.Compile();
            var personViewModel = mappingFunc(person);

            Assert.AreEqual("Dave", personViewModel.FirstName);
        }

        [TestMethod]
        public void Expression_validator_works_for_correct_mapping()
        {
            var report = new List<string>();
            var isValid = Library.Validator.Validate(PersonMapper.PersonModelToPersonViewModel, report);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Expression_validator_works_for_incorrect_mapping()
        {
            var report = new List<string>();
            var isValid = Library.Validator.Validate(PersonMapper.IncorrectPersonModelToPersonViewModel, report);
            Assert.IsFalse(isValid);
            Assert.AreEqual(2, report.Count);
        }

        [TestMethod]
        public void Expression_validator_works_for_incorrect_mapped_pets()
        {
            var report = new List<string>();
            var isValid = Library.Validator.Validate(PersonMapper.IncorrectPetMappingPersonModelToPersonViewModel, report);
            Assert.IsFalse(isValid);
            Assert.AreEqual(1, report.Count);
        }

        [TestMethod]
        public void Expression_validator_works_for_incorrect_mapped_pet_name()
        {
            var report = new List<string>();
            var isValid = Library.Validator.Validate(PersonMapper.IncorrectPetNameMappingPersonModelToPersonViewModel, report);
            Assert.IsFalse(isValid);
            Assert.AreEqual(1, report.Count);
            Assert.AreEqual("- Pets.Name", report[0]);
        }

        [TestMethod]
        public void Expression_validator_works_for_assembly()
        {
            var assembly = Assembly.GetAssembly(typeof(PersonMapper));
            Debug.Assert(assembly != null);
            var report = new List<string>();
            var isValid = Library.Validator.ValidateAssembly(assembly, report);
            Assert.IsFalse(isValid);
            Assert.AreEqual(7, report.Count);
            Assert.AreEqual("Dnote.MappingValidator.Main.PersonMapper.IncorrectPersonModelToPersonViewModel", report[0]);
            Assert.AreEqual("- FirstChildName", report[1]);
            Assert.AreEqual("- Street", report[2]);
            Assert.AreEqual("Dnote.MappingValidator.Main.PersonMapper.IncorrectPetMappingPersonModelToPersonViewModel", report[3]);
            Assert.AreEqual("- Pets", report[4]);
            Assert.AreEqual("Dnote.MappingValidator.Main.PersonMapper.IncorrectPetNameMappingPersonModelToPersonViewModel", report[5]);
            Assert.AreEqual("- Pets.Name", report[6]);
        }

        [TestMethod]
        public void Expression_validator_works_for_mapping_excluding_last_name()
        {
            var report = new List<string>();
            var isValid = Library.Validator.Validate(PersonMapper.CorrectMappingPersonModelToPersonViewModelExcludingLastName, report, 
                nameof(PersonViewModel.LastName));

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Expression_validator_works_for_mapping_excluding_pet_name()
        {
            var report = new List<string>();
            var isValid = Library.Validator.Validate(PersonMapper.CorrectMappingPersonModelToPersonViewModelExcludingPetName, report,
                nameof(PersonViewModel.Pets) + "." + nameof(PetViewModel.Name));

            Assert.IsTrue(isValid);
        }
    }
}