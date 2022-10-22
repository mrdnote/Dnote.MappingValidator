using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using Dnote.MappingValidator.Sample;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dnote.MappingValidator.Tests
{
    [TestClass]
    public class AssemblyValidatorTests
    {
        [TestMethod]
        public void Validators_work_for_assembly()
        {
            var assembly = Assembly.GetAssembly(typeof(ExpressionMappers));
            Debug.Assert(assembly != null);
            var report = new List<string>();
            var isValid = Library.Validator.ValidateAssembly(assembly, report);
            Assert.IsFalse(isValid);
            Assert.AreEqual(18, report.Count);

            var idx = report.IndexOf("Dnote.MappingValidator.Sample.ExpressionMappers.IncorrectPersonModelToPersonViewModel");
            Assert.AreNotEqual(-1, idx);
            Assert.AreEqual("- FirstChildName", report[idx + 1]);
            Assert.AreEqual("- Street", report[idx + 2]);

            idx = report.IndexOf("Dnote.MappingValidator.Sample.ExpressionMappers.IncorrectPetMappingPersonModelToPersonViewModel");
            Assert.AreNotEqual(-1, idx);
            Assert.AreEqual("- Pets", report[idx + 1]);

            idx = report.IndexOf("Dnote.MappingValidator.Sample.ExpressionMappers.IncorrectPetNameMappingPersonModelToPersonViewModel");
            Assert.AreNotEqual(-1, idx);
            Assert.AreEqual("- Pets.Name", report[idx + 1]);

            idx = report.IndexOf("Dnote.MappingValidator.Sample.ProceduralMappers.IncorrectPersonModelToPersonViewModel");
            Assert.AreNotEqual(-1, idx);
            Assert.AreEqual("- FirstChildName", report[idx + 1]);
            Assert.AreEqual("- Street", report[idx + 2]);

            idx = report.IndexOf("Dnote.MappingValidator.Sample.ProceduralMappers.IncorrectPetMappingPersonModelToPersonViewModel");
            Assert.AreNotEqual(-1, idx);
            Assert.AreEqual("- Pets", report[idx + 1]);

            idx = report.IndexOf("Dnote.MappingValidator.Sample.ProceduralMappers.IncorrectPetNameMappingPersonModelToPersonViewModel");
            Assert.AreNotEqual(-1, idx);
            Assert.AreEqual("- Pets.Name", report[idx + 1]);

            idx = report.IndexOf("Dnote.MappingValidator.Sample.InvalidNestedExpressionMappers.PersonMapper");
            Assert.AreNotEqual(-1, idx);
            Assert.AreEqual("- Pets.Name", report[idx + 1]);

            idx = report.IndexOf("Dnote.MappingValidator.Sample.InvalidNestedExpressionMappers.PetMapper");
            Assert.AreNotEqual(-1, idx);
            Assert.AreEqual("- Name", report[idx + 1]);
        }
    }
}
