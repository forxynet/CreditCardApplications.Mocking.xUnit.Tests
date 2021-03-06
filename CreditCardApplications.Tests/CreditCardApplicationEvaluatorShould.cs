using System;
using Xunit;
using Moq;
using System.Collections.Generic;
using Moq.Protected;

namespace CreditCardApplications.Tests
{
    public class CreditCardApplicationEvaluatorShould
    {
        private readonly Mock<IFrequentFlyerNumberValidator> mockValidator;
        private readonly CreditCardApplicationEvaluator sut;
        public CreditCardApplicationEvaluatorShould() {
            mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.SetupAllProperties();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            mockValidator.Setup(x=>x.IsValid(It.IsAny<string>())).Returns(true);

            sut = new CreditCardApplicationEvaluator(mockValidator.Object);
        }

        [Fact]
        public void AcceptHighIncomeApplications()
        {     
            var application = new CreditCardApplication { GrossAnnualIncome = 100_000 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoAccepted, decision);
        }

        [Fact]
        public void ReferYoungApplications()
        {          
            mockValidator.DefaultValue = DefaultValue.Mock;

            var application = new CreditCardApplication { Age = 19 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void DeclineLowIncomeApplications()
        {
            
            mockValidator.Setup(
                          x => x.IsValid(It.IsRegex("[a-z]")))
                         .Returns(true);
                      
            var application = new CreditCardApplication
            {
                GrossAnnualIncome = 19_999,
                Age = 42,
                FrequentFlyerNumber = "y"
            };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

        [Fact]
        public void ReferInvalidFrequentFlyerApplications()
        {  
            var application = new CreditCardApplication();

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        
        [Fact]
        public void ReferWhenLicenseKeyExpired()
        {           
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("EXPIRED");

            mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { Age = 42 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        string GetLicenseKeyExpiryString()
        {
            // E.g. read from vendor-supplied constants file
            return "EXPIRED";
        }

        [Fact]
        public void UseDetailedLookupForOlderApplications()
        {
            var application = new CreditCardApplication { Age = 30 };

            sut.Evaluate(application);

            Assert.Equal(ValidationMode.Detailed, mockValidator.Object.ValidationMode);
        }

        [Fact]
        public void ValidateFrequentFlyerNumberForLowIncomeApplications() {
           
            var application = new CreditCardApplication {
                FrequentFlyerNumber = "q"
            };

            sut.Evaluate(application);

            mockValidator.Verify(x => x.IsValid(It.IsAny<string>()),Times.Once);
        }

        [Fact]
        public void NotValidateFrequentFlayerNumberForHighIncomeApplication() {
                      
            var application = new CreditCardApplication { GrossAnnualIncome = 100_000 };

            sut.Evaluate(application);

            mockValidator.Verify(x => x.IsValid(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void CheckLicenseKeyForLowIncomeApplication() {            
            
            var application = new CreditCardApplication { GrossAnnualIncome = 99_000 };

            sut.Evaluate(application);

            mockValidator.VerifyGet(x => x.ServiceInformation.License.LicenseKey, Times.Once);
        }

        [Fact]
        public void SetDetailedLookupForOlderApplications() {
                       
            var application = new CreditCardApplication { Age = 30 };
            sut.Evaluate(application);
            mockValidator.VerifySet(x=>x.ValidationMode = It.IsAny<ValidationMode>(), Times.Once);
            mockValidator.Verify(x => x.IsValid(null), Times.Once);
        }

        [Fact]
        public void ReferWhenFrequentFlyerValidationError() {                       
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Throws(new Exception("Custome message"));
            var application = new CreditCardApplication { Age = 42 };
            CreditCardApplicationDecision decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void IncrementLookupCount() {
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>()))
                .Returns(true)
                .Raises(x => x.ValidatorLookupPerformed += null, EventArgs.Empty);

            var application = new CreditCardApplication { FrequentFlyerNumber = "x", Age = 25 };
            sut.Evaluate(application);
            Assert.Equal(1, sut.ValidatorLookupCount);        
        }

        [Fact]
        public void ReferInvalidFrequentFlyerApplications_ReturnValuesSequence() {
           
            mockValidator.SetupSequence(x => x.IsValid(It.IsAny<string>())).Returns(false).Returns(true);

            var application = new CreditCardApplication { Age = 25 };

            CreditCardApplicationDecision firstDecision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, firstDecision);

            CreditCardApplicationDecision secondDecision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, secondDecision);
        }

        [Fact]
        public void ReferInvalidFrequentFlyerApplications_MultibleCallsSequesnce() {

            var frequentFlyerNumbersPassede = new List<string>();
            mockValidator.Setup(x => x.IsValid(Capture.In(frequentFlyerNumbersPassede)));
                      
            var application1 = new CreditCardApplication { Age = 25, FrequentFlyerNumber = "aa" };
            var application2 = new CreditCardApplication { Age = 25, FrequentFlyerNumber = "bb" };
            var application3 = new CreditCardApplication { Age = 25, FrequentFlyerNumber = "cc" };

            sut.Evaluate(application1);
            sut.Evaluate(application2);
            sut.Evaluate(application3);

            Assert.Equal(new List<string> { "aa", "bb", "cc" }, frequentFlyerNumbersPassede);
        }

        [Fact]
        public void ReferFraudRisk() {
                      
            Mock<FraudLookup> mockFraudLookup = new Mock<FraudLookup>();            

            mockFraudLookup.Protected()
                .Setup<bool>("CheckApplication",ItExpr.IsAny<CreditCardApplication>())
                .Returns(true);

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object, mockFraudLookup.Object);

            var application = new CreditCardApplication();

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHumanFraudRisk, decision);

         }

        [Fact]
        public void LinqToMock() {

            IFrequentFlyerNumberValidator mockValidator = 
                Mock.Of<IFrequentFlyerNumberValidator>(
                    validator =>
                    validator.ServiceInformation.License.LicenseKey == "OK" &&
                    validator.IsValid(It.IsAny<string>()) == true
                );

            var sut = new CreditCardApplicationEvaluator(mockValidator);

            var application = new CreditCardApplication { Age = 25 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }
    }
}
