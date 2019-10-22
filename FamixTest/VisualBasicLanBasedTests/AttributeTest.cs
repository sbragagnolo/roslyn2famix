﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FamixTest.VisualBasic {
    [TestClass]
    public class AttributeTest : LanProjectVisualBasicLoader {
        [TestMethod]
        public void SingleAttributeWasIngested() {
            Assert.IsNotNull(importer.Attributes.Named("SampleProject.Basic.Attribute.i"));
        }
        [TestMethod]
        public void MultipleAttributeWasIngested() {
            Assert.IsNotNull(importer.Attributes.Named("SampleProject.Basic.Attribute.k"));
        }
        [TestMethod]
        public void MethodAccessesAttribute() {
            Assert.AreEqual(2, importer.Methods.Named("SampleProject.Basic.Attribute.Accessor()").Accesses.Count);
        }
    }
}
