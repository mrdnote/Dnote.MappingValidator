using System.Collections.Generic;

using Dnote.MappingValidator.Sample;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dnote.MappingValidator.Tests
{
    [TestClass]
    public class ProceduralMappersTests
    {
        [TestMethod]
        public void Procedural_validator_works_for_correct_mapping()
        {
            var report = new List<string>();
            var isValid = Library.Validator.ValidateMethod(ProceduralMappers.PersonModelToPersonViewModel, report, false);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Procedural_validator_works_for_incorrect_mapping()
        {
            var report = new List<string>();
            var isValid = Library.Validator.ValidateMethod(ProceduralMappers.IncorrectPersonModelToPersonViewModel, report, false);
            Assert.IsFalse(isValid);
            Assert.AreEqual(2, report.Count);
        }

        [TestMethod]
        public void Procedural_validator_works_for_incorrect_mapped_pets()
        {
            var report = new List<string>();
            var isValid = Library.Validator.ValidateMethod(ProceduralMappers.IncorrectPetMappingPersonModelToPersonViewModel, report, false);
            Assert.IsFalse(isValid);
            Assert.AreEqual(1, report.Count);
        }

        [TestMethod]
        public void Procedural_validator_works_for_incorrect_mapped_pet_name()
        {
            var report = new List<string>();
            var isValid = Library.Validator.ValidateMethod(ProceduralMappers.IncorrectPetNameMappingPersonModelToPersonViewModel, report, false);
            Assert.IsFalse(isValid);
            Assert.AreEqual(1, report.Count);
            Assert.AreEqual("- Pets.Name", report[0]);
        }
    }
}
