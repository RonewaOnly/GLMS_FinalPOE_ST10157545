using GLMS.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLMS.TEST
{
    public class ContractWorkflowTests
    {
        [Theory]
        [InlineData(ContractStatus.Active, true)]
        [InlineData(ContractStatus.Draft, true)]
        [InlineData(ContractStatus.Expired, false)]
        [InlineData(ContractStatus.OnHold, false)]
        public void CanCreateServiceRequest_MatchesExpectedRule(
    ContractStatus status, bool expectedCanCreate)
        {
            var contract = new Contract
            {
                Id = 1,
                ClientId = 1,
                StartDate = DateTime.Today.AddDays(-30),
                EndDate = DateTime.Today.AddDays(30),
                Status = status,
                ServiceLevel = "Standard"
            };
            Assert.Equal(expectedCanCreate, contract.CanCreateServiceRequest);
        }
        [Fact]
        public void ExpiredContract_CannotCreateServiceRequest()
        {
            var contract = new Contract { Status = ContractStatus.Expired };
            Assert.False(contract.CanCreateServiceRequest);
        }
        [Fact]
        public void OnHoldContract_CannotCreateServiceRequest()
        {
            var contract = new Contract { Status = ContractStatus.OnHold };
            Assert.False(contract.CanCreateServiceRequest);
        }
        [Fact]
        public void ActiveContract_IsActivePropertyIsTrue()
        {
            var contract = new Contract { Status = ContractStatus.Active };
            Assert.True(contract.IsActive);
        }
        [Fact]
        public void DraftContract_IsActivePropertyIsFalse()
        {
            var contract = new Contract { Status = ContractStatus.Draft };
            Assert.False(contract.IsActive);
        }
    }
}
