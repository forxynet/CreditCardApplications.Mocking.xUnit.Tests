using System;
using System.Collections.Generic;
using System.Text;

namespace CreditCardApplications {
    public class FraudLookup {
        public bool IsFraudRisk(CreditCardApplication application) {
           return CheckApplication(application);
        }
        virtual protected bool CheckApplication(CreditCardApplication application) {
            return application.LastName == "Smith" ? true : false;
        }
    }
}
