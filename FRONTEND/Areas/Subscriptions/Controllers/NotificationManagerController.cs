using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BAL.Audit;
using DAL.AUDIT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FRONTEND.Areas.Subscriptions.Controllers
{
    [Authorize]
    public class NotificationManagerController : Controller
    {
        private readonly IUsersOnlineRepository usersOnlineRepository;
        private readonly AuditDbContext auditContext;

        public NotificationManagerController(IUsersOnlineRepository usersOnlineRepository, AuditDbContext auditContext)
        {
            this.usersOnlineRepository = usersOnlineRepository;
            this.auditContext = auditContext;
        }

        [HttpPost]
        public async Task<JsonResult> MarkAsReadToggle(int notificationID)
        {
            var notification = await auditContext.ListingNotification.Where(i => i.ListingNotificationID == notificationID).FirstOrDefaultAsync();
            var entityType = notification.EntityType;
            var markAsRead = notification.MarkAsRead;
            await usersOnlineRepository.NotificationMarkAsReadToggleAsyn(notificationID, entityType);
            var unreadNotificationCount = await auditContext.ListingNotification.Where(i => i.NotifierGUID == notification.NotifierGUID && i.MarkAsRead == false).CountAsync();
            return Json("{" + notificationID + "}" + "(" + entityType + ")" + "[" + markAsRead + "]" + "(-" + unreadNotificationCount + "-)");
        }

        [HttpPost]
        public async Task<JsonResult> MarkAllAsRead(string getListOfListingNotifictaionIds)
        {
            var notificationIds = getListOfListingNotifictaionIds.Split(',').Select(Int32.Parse).ToList();

            IList<int> markedAsReadNotificationIds = new List<int>();

            foreach(var item in notificationIds)
            {
                var notification = await auditContext.ListingNotification.Where(i => i.ListingNotificationID == item).FirstOrDefaultAsync();

                if(notification != null && notification.MarkAsRead == false)
                {
                    await usersOnlineRepository.NotificationMarkSingleAsReadAsync(notification.ListingNotificationID);
                    markedAsReadNotificationIds.Add(item);
                }
            }

            string result = string.Join(",", markedAsReadNotificationIds.Select(n => n.ToString()).ToArray());


            return Json(result);
        }
    }
}
    