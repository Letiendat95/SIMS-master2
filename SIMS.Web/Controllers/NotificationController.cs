using Microsoft.AspNetCore.Mvc;
using SIMS.Web.Helpers;
using SIMS.Web.Services;

namespace SIMS.Web.Controllers
{
    [Authorize("Admin", "Faculty", "Student")]
    public class NotificationController : Controller
    {
        private readonly NotificationService _notificationService;

        public NotificationController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int userId))
                return RedirectToAction("Login", "Account");

            var notifications = await _notificationService.GetByUserAsync(userId);
            return View(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int userId))
                return RedirectToAction("Login", "Account");

            // Chỉ được đánh dấu thông báo của chính mình
            await _notificationService.MarkAsReadAsync(id, userId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            if (int.TryParse(HttpContext.Session.GetString("UserId"), out int userId))
            {
                await _notificationService.MarkAllAsReadAsync(userId);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int userId))
                return Json(0);

            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Json(count);
        }
    }
}
