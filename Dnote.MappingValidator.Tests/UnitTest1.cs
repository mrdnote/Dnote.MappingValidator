using System.Reflection;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Dnote.MappingValidator.Sample;

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

            var mappingFunc = ExpressionMappings.PersonModelToPersonViewModel.Compile();
            var personViewModel = mappingFunc(person);

            Assert.AreEqual("Dave", personViewModel.FirstName);
        }

        [TestMethod]
        public void Expression_validator_works_for_correct_mapping()
        {
            var report = new List<string>();
            var isValid = Library.Validator.Validate(ExpressionMappings.PersonModelToPersonViewModel, report);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Expression_validator_works_for_incorrect_mapping()
        {
            var report = new List<string>();
            var isValid = Library.Validator.Validate(ExpressionMappings.IncorrectPersonModelToPersonViewModel, report);
            Assert.IsFalse(isValid);
            Assert.AreEqual(2, report.Count);
        }

        [TestMethod]
        public void Expression_validator_works_for_incorrect_mapped_pets()
        {
            var report = new List<string>();
            var isValid = Library.Validator.Validate(ExpressionMappings.IncorrectPetMappingPersonModelToPersonViewModel, report);
            Assert.IsFalse(isValid);
            Assert.AreEqual(1, report.Count);
        }

        [TestMethod]
        public void Expression_validator_works_for_incorrect_mapped_pet_name()
        {
            var report = new List<string>();
            var isValid = Library.Validator.Validate(ExpressionMappings.IncorrectPetNameMappingPersonModelToPersonViewModel, report);
            Assert.IsFalse(isValid);
            Assert.AreEqual(1, report.Count);
            Assert.AreEqual("- Pets.Name", report[0]);
        }

        [TestMethod]
        public void Validators_work_for_assembly()
        {
            var assembly = Assembly.GetAssembly(typeof(ExpressionMappings));
            Debug.Assert(assembly != null);
            var report = new List<string>();
            var isValid = Library.Validator.ValidateAssembly(assembly, report);
            Assert.IsFalse(isValid);
            Assert.AreEqual(14, report.Count);
            
            var idx = report.IndexOf("Dnote.MappingValidator.Sample.ExpressionMappings.IncorrectPersonModelToPersonViewModel");
            Assert.AreNotEqual(-1, idx);
            Assert.AreEqual("- FirstChildName", report[idx + 1]);
            Assert.AreEqual("- Street", report[idx + 2]);

            idx = report.IndexOf("Dnote.MappingValidator.Sample.ExpressionMappings.IncorrectPetMappingPersonModelToPersonViewModel");
            Assert.AreNotEqual(-1, idx);
            Assert.AreEqual("- Pets", report[idx + 1]);
            
            idx = report.IndexOf("Dnote.MappingValidator.Sample.ExpressionMappings.IncorrectPetNameMappingPersonModelToPersonViewModel");
            Assert.AreNotEqual(-1, idx);
            Assert.AreEqual("- Pets.Name", report[idx + 1]);

            idx = report.IndexOf("Dnote.MappingValidator.Sample.ProceduralMappings.IncorrectPersonModelToPersonViewModel");
            Assert.AreNotEqual(-1, idx);
            Assert.AreEqual("- FirstChildName", report[idx + 1]);
            Assert.AreEqual("- Street", report[idx + 2]);
            
            idx = report.IndexOf("Dnote.MappingValidator.Sample.ProceduralMappings.IncorrectPetMappingPersonModelToPersonViewModel");
            Assert.AreNotEqual(-1, idx);
            Assert.AreEqual("- Pets", report[idx + 1]);
            
            idx = report.IndexOf("Dnote.MappingValidator.Sample.ProceduralMappings.IncorrectPetNameMappingPersonModelToPersonViewModel");
            Assert.AreNotEqual(-1, idx);
            Assert.AreEqual("- Pets.Name", report[idx + 1]);
        }

        [TestMethod]
        public void Expression_validator_works_for_mapping_excluding_last_name()
        {
            var report = new List<string>();
            var isValid = Library.Validator.Validate(ExpressionMappings.CorrectMappingPersonModelToPersonViewModelExcludingLastName, report, 
                nameof(PersonViewModel.LastName));

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Expression_validator_works_for_mapping_excluding_pet_name()
        {
            var report = new List<string>();
            var isValid = Library.Validator.Validate(ExpressionMappings.CorrectMappingPersonModelToPersonViewModelExcludingPetName, report,
                nameof(PersonViewModel.Pets) + "." + nameof(PetViewModel.Name));

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Procedural_validator_works_for_correct_mapping()
        {
            var report = new List<string>();
            var isValid = Library.Validator.ValidateMethod(ProceduralMappings.PersonModelToPersonViewModel, report);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Procedural_validator_works_for_incorrect_mapping()
        {
            var report = new List<string>();
            var isValid = Library.Validator.ValidateMethod(ProceduralMappings.IncorrectPersonModelToPersonViewModel, report);
            Assert.IsFalse(isValid);
            Assert.AreEqual(2, report.Count);
        }

        [TestMethod]
        public void Procedural_validator_works_for_incorrect_mapped_pets()
        {
            var report = new List<string>();
            var isValid = Library.Validator.ValidateMethod(ProceduralMappings.IncorrectPetMappingPersonModelToPersonViewModel, report);
            Assert.IsFalse(isValid);
            Assert.AreEqual(1, report.Count);
        }

        [TestMethod]
        public void Procedural_validator_works_for_incorrect_mapped_pet_name()
        {
            var report = new List<string>();
            var isValid = Library.Validator.ValidateMethod(ProceduralMappings.IncorrectPetNameMappingPersonModelToPersonViewModel, report);
            Assert.IsFalse(isValid);
            Assert.AreEqual(1, report.Count);
            Assert.AreEqual("- Pets.Name", report[0]);
        }
    }
}