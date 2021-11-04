using CreditCardApplications;
using Moq;
using Xunit;

namespace CreditCardSApplications.Tests {
    public class CreditCardApplicationEvaluatorTest {

        private readonly CreditCardApplicationEvaluator _sut;
        private readonly Mock<IFrequentFlyerNumberValidator> _mockValidate;

        public CreditCardApplicationEvaluatorTest() {
           
            _mockValidate = new Mock<IFrequentFlyerNumberValidator>();
            _sut = new CreditCardApplicationEvaluator(_mockValidate.Object);
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

        [Fact]
        public void DeclineLowIncomeApplication() {
            var application = new CreditCardApplication { GrossAnnualIncome = 19_000 };
        }
    }
}
