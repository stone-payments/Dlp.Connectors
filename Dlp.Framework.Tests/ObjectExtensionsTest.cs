using Dlp.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Dlp.Sdk.Tests {

    [ExcludeFromCodeCoverage]
    public sealed class Report {

        public Report() { }

        public string Field { get; set; }

        public string Message { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class MerchantReference : AbstractMerchant {

        public MerchantReference() { this.ReportCollection = new List<Report>(); }

        public string Name { get; set; }

        public IList<Report> ReportCollection { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public abstract class AbstractMerchant {

        public Guid MerchantKey { get; set; }
    }

    public class BaseMerchant {

        public BaseMerchant() { }

        public string MerchantName { get; set; }
    }

    public class LegalMerchant : BaseMerchant {

        public LegalMerchant() { }

        public Nullable<decimal> Count { get; set; }
    }

    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ObjectExtensionsTest {

        [TestMethod]
        public void DiffProperties_Test() {

            LegalMerchant lm = new LegalMerchant();

            lm.MerchantName = "Legal";
            lm.Count = 100M;

            LegalMerchant bm = new LegalMerchant();

            bm.MerchantName = "Base";
            bm.Count = 100;

            IEnumerable<DiffProperty> p = this.GetDiff<BaseMerchant>(lm, bm);
        }

        private IEnumerable<DiffProperty> GetDiff<T>(T obj1, T obj2) {

            return Dlp.Framework.ObjectExtensions.DiffProperties<object>(obj1, obj2);
        }

        [TestMethod]
        public void CloneObject() {

            MerchantReference merchantReference = new MerchantReference();

            merchantReference.Name = "Configuração";
            merchantReference.MerchantKey = Guid.NewGuid();

            merchantReference.ReportCollection.Add(new Report() { Field = "Teste", Message = "Conteúdo" });
            merchantReference.ReportCollection.Add(new Report() { Field = "Outro teste", Message = "Outro conteúdo" });

            MerchantReference clonedMerchantReference = merchantReference.Clone();

            Assert.IsNotNull(clonedMerchantReference);

            Assert.AreEqual(clonedMerchantReference.Name, merchantReference.Name);

            clonedMerchantReference.Name = "Edição";

            Assert.AreNotEqual(clonedMerchantReference.Name, merchantReference.Name);
        }

        [TestMethod]
        public void CloneNullObject() {

            MerchantReference actual = ObjectExtensions.Clone<MerchantReference>(null);

            Assert.IsNull(actual);
        }
    }


}
