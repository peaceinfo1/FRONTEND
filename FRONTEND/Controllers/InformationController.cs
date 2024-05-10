using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DAL.CATEGORIES;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace FRONTEND.Controllers
{
    public class InformationController : Controller
    {
        private readonly CategoriesDbContext categoriesContext;
        public InformationController(CategoriesDbContext categoriesContext)
        {
            this.categoriesContext = categoriesContext;
        }


        [HttpGet]
        [Route("/Information/{PageURL}")]
        public async Task<IActionResult> Index(string PageURL)
        {
            var page = await categoriesContext.Pages.Where(p => p.URL == PageURL).FirstOrDefaultAsync();

            return View(page);
        }

        public IActionResult Contact()
        {
            return View();
        }
    }
}
