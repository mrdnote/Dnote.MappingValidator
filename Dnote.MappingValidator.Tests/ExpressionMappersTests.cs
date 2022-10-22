using System.Collections.Generic;

using Dnote.MappingValidator.Sample;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dnote.MappingValidator.Tests
{
    [TestClass]
    public class ExpressionMappersTests
    {
        [TestMethod]
        public void Expression_validator_works_for_correct_mapping()
        {
            var report = new List<string>();
            var isValid = Library.Validator.Validate(ExpressionMappers.PersonModelToPersonViewModel, report);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Expression_validator_works_for_nested_mapping()
        {
            var report = new List<string>();
            var isValid = Library.Validator.Validate(NestedExpressionMappers.PersonMapper, report);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Expression_validator_works_for_invalid_nested_mapping()
        {
            var report = new List<string>();
            var isValid = Library.Validator.Validate(InvalidNestedExpressionMappers.PersonMapper, report);

            Assert.IsFalse(isValid);
            Assert.AreEqual(1, report.Count);
            Assert.AreEqual("- Pets.Name", report[0]);
        }

        [TestMethod]
        public void Expression_validator_works_for_incorrect_mapping()
        {
            var report = new List<string>();
            var isValid = Library.Validator.Validate(ExpressionMappers.IncorrectPersonModelToPersonViewModel, report);
            Assert.IsFalse(isValid);
            Assert.AreEqual(2, report.Count);
        }

        [TestMethod]
        public void Expression_validator_works_for_incorrect_mapped_pets()
        {
            var report = new List<string>();
            var isValid = Library.Validator.Validate(ExpressionMappers.IncorrectPetMappingPersonModelToPersonViewModel, report);
            Assert.IsFalse(isValid);
            Assert.AreEqual(1, report.Count);
        }

        [TestMethod]
        public void Expression_validator_works_for_incorrect_mapped_pet_name()
        {
            var report = new List<string>();
            var isValid = Library.Validator.Validate(ExpressionMappers.IncorrectPetNameMappingPersonModelToPersonViewModel, report);
            Assert.IsFalse(isValid);
            Assert.AreEqual(1, report.Count);
            Assert.AreEqual("- Pets.Name", report[0]);
        }

        [TestMethod]
        public void Expression_validator_works_for_mapping_excluding_last_name()
        {
            var report = new List<string>();
            var isValid = Library.Validator.Validate(ExpressionMappers.CorrectMappingPersonModelToPersonViewModelExcludingLastName, report,
                false, nameof(PersonViewModel.LastName));

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Expression_validator_works_for_mapping_excluding_pet_name()
        {
            var report = new List<string>();
            var isValid = Library.Validator.Validate(ExpressionMappers.CorrectMappingPersonModelToPersonViewModelExcludingPetName, report,
                false, nameof(PersonViewModel.Pets) + "." + nameof(PetViewModel.Name));

            Assert.IsTrue(isValid);
        }
    }
}