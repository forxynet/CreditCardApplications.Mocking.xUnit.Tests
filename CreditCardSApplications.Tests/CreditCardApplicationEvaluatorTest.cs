using CreditCardApplications;
using System;
using Xunit;

namespace CreditCardSApplications.Tests {
    public class CreditCardApplicationEvaluatorTest {
        private CreditCardApplicationEvaluator _sut;

        public CreditCardApplicationEvaluatorTest() {
            _sut = new CreditCardApplicationEvaluator();
        }
        [Fact]
        public void AcceptHightIncomeApplications() {            
            var application = new CreditCardApplication { GrossAnnualIncome = 100_000 };
            CreditCardApplicationDecision decision = _sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoAccepted, decision);
        }
        [Fact]
        public void ReferYoungApplications() {
            var application = new CreditCardApplication { Age = 17 };
            CreditCardApplicationDecision decision = _sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }
    }
}
