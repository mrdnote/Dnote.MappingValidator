using System.Reflection;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Dnote.MappingValidator.Sample;

namespace Dnote.MappingValidator.Tests
{
    [TestClass]
    public class BaseLineTests
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

            var mappingFunc = ExpressionMappers.PersonModelToPersonViewModel.Compile();
            var personViewModel = mappingFunc(person);

            Assert.AreEqual("Dave", personViewModel.FirstName);
        }
    }
}