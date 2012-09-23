using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMemory.Indexes;
using NMemory.Tables;

namespace NMemory.Test.Indexes
{
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
