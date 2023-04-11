using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoyaltyProgramServiceTests.Mocks
{
    public class NotificationsMock : ControllerBase
    {
        public static bool ReceivedNotification = false;

        [HttpPost("/notify")]
        public OkResult Notify()
        {
            ReceivedNotification = true;
            return Ok();
        }
    }
}
