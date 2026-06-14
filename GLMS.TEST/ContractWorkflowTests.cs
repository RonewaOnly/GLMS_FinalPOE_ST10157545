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
        public void CanCreateSR_MatchesExpected(ContractStatus status, bool expected)
            => Assert.Equal(expected, new Contract { Status = status }.CanCreateServiceRequest);


        [Fact] public void Expired_BlocksSR() => Assert.False(new Contract { Status = ContractStatus.Expired }.CanCreateServiceRequest);
        [Fact] public void OnHold_BlocksSR() => Assert.False(new Contract { Status = ContractStatus.OnHold }.CanCreateServiceRequest);
        [Fact] public void Active_AllowsSR() => Assert.True(new Contract { Status = ContractStatus.Active }.CanCreateServiceRequest);
        [Fact] public void Draft_AllowsSR() => Assert.True(new Contract { Status = ContractStatus.Draft }.CanCreateServiceRequest);
    }
}
