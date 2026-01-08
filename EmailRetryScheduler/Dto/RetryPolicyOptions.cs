using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailRetryScheduler.Dto
{
    public class RetryPolicyOptions
    {
        public int RetryIntervalSeconds { get; set; } = 60;
        public int MaxAttempts { get; set; } = 8;
        public List<int> BackoffScheduleMinutes { get; set; } = new();
    }

}
