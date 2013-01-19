// -----------------------------------------------------------------------------------
// <copyright file="RelationKeyConverterFactoryFixture.cs" company="NMemory Team">
//     Copyright (C) 2012-2013 NMemory Team
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// -----------------------------------------------------------------------------------

namespace NMemory.Test.Indexes
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NMemory.Indexes;
    using NMemory.Tables;

    [TestClass]
    public class RelationKeyConverterFactoryFixture
    {
        private class PrimaryRow
        {
            public int P11 { get; set; }
            public int P12 { get; set; }
            public int? P13 { get; set; }
        }

        private class ForeignRow
        {
            public int P21 { get; set; }
            public int P22 { get; set; }
            public int P23 { get; set; }
        }

        [TestMethod]
        public void CreatePrimaryToForeignConversion()
        {
            DefaultKeyInfoFactory factory = new DefaultKeyInfoFactory();

            var primary = factory.Create((PrimaryRow x) => new { K11 = x.P11, K12 = x.P12, K13 = x.P13 });
            var foreign = factory.Create((ForeignRow x) => new { K21 = x.P21, K22 = x.P22, K23 = x.P23 });

            // Field mapping (no mirror):
            // P11 - P22
            // P12 - P23
            // P13 - P21
            var converter = RelationKeyConverterFactory.CreatePrimaryToForeignConverter(primary, foreign,
                new RelationConstraint<PrimaryRow, ForeignRow, int>(x => x.P11, x => x.P22),
                new RelationConstraint<PrimaryRow, ForeignRow, int>(x => x.P12, x => x.P23),
                new RelationConstraint<PrimaryRow, ForeignRow, int>(x => (int)x.P13, x => x.P21));
            
            var result = converter.Invoke(new { K11 = 1, K12 = 2, K13 = (int?)3 });

            Assert.AreEqual(2, result.K21);
            Assert.AreEqual(3, result.K22);
            Assert.AreEqual(1, result.K23);
        }

        [TestMethod]
        public void CreateForeignToPrimaryConversion()
        {
            DefaultKeyInfoFactory factory = new DefaultKeyInfoFactory();

            var primary = factory.Create((PrimaryRow x) => new { K11 = x.P11, K12 = x.P12, K13 = x.P13 });
            var foreign = factory.Create((ForeignRow x) => new { K21 = x.P21, K22 = x.P22, K23 = x.P23 });

            // Field mapping (no mirror):
            // P11 - P22
            // P12 - P23
            // P13 - P21
            var converter = RelationKeyConverterFactory.CreateForeignToPrimaryConverter(primary, foreign,
                new RelationConstraint<PrimaryRow, ForeignRow, int>(x => x.P11, x => x.P22),
                new RelationConstraint<PrimaryRow, ForeignRow, int>(x => x.P12, x => x.P23),
                new RelationConstraint<PrimaryRow, ForeignRow, int>(x => (int)x.P13, x => x.P21));

            var result = converter.Invoke(new { K21 = 1, K22 = 2, K23 = 3 });

            Assert.AreEqual(3, result.K11);
            Assert.AreEqual(1, result.K12);
            Assert.AreEqual(2, result.K13);
        }
    }
}
