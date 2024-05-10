using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FRONTEND.Controllers
{
    public class PrivateFilesController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), "AppData", "part2.mp4");
            return PhysicalFile(file, "application/octet-stream", enableRangeProcessing: true);
        }
    }
}
