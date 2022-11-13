using System.Collections.Generic;

using Dnote.MappingValidator.Sample;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dnote.MappingValidator.Tests
{
    [TestClass]
    public class FunctionMappersTests
    {
        [TestMethod]
        public void Function_validator_works_for_correct_mapping()
        {
            var report = new List<string>();
            var isValid = Library.Validator.ValidateFunction(FunctionMappers.PersonModelToPersonViewModel, report, false);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Function_validator_works_for_correct_mapping_with_extra_parameter()
        {
            var report = new List<string>();
            var isValid = Library.Validator.ValidateFunction(FunctionMappers.PersonModelToPersonViewModelExtraParameter, report, false);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Function_validator_works_for_incorrect_mapping()
        {
            var report = new List<string>();
            var isValid = Library.Validator.ValidateFunction(FunctionMappers.IncorrectPersonModelToPersonViewModel, report, false);
            Assert.IsFalse(isValid);
            Assert.AreEqual(2, report.Count);
        }

        [TestMethod]
        public void Function_validator_works_for_incorrect_mapped_pets()
        {
            var report = new List<string>();
            var isValid = Library.Validator.ValidateFunction(FunctionMappers.IncorrectPetMappingPersonModelToPersonViewModel, report, false);
            Assert.IsFalse(isValid);
            Assert.AreEqual(1, report.Count);
        }

        [TestMethod]
        public void Function_validator_works_for_incorrect_mapped_pet_name()
        {
            var report = new List<string>();
            var isValid = Library.Validator.ValidateFunction(FunctionMappers.IncorrectPetNameMappingPersonModelToPersonViewModel, report, false);
            Assert.IsFalse(isValid);
            Assert.AreEqual(1, report.Count);
            Assert.AreEqual("- Pets.Name", report[0]);
        }
    }
}
