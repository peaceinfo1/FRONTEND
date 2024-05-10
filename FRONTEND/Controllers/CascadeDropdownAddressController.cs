using System.Linq;
using BAL.Listings;
using DAL.LISTING;
using DAL.SHARED;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRONTEND.Controllers
{
    public class CascadeDropdownAddressController : Controller
    {
        private readonly ListingDbContext listingContext;
        private readonly SharedDbContext sharedManager;
        private readonly IListingManager listingManager;

        public CascadeDropdownAddressController(ListingDbContext listingContext, SharedDbContext sharedManager, IListingManager listingManager)
        {
            this.listingContext = listingContext;
            this.sharedManager = sharedManager;
            this.listingManager = listingManager;
        }

        // Begin: Cascade Dropdown For Countries
        public JsonResult fetchCountries()
        {
            var selCountries = sharedManager.Country
                .OrderBy(c => c.Name)
                .Select(c => new { value = c.CountryID, text = c.Name });
            return Json(new SelectList(selCountries, "value", "text"));
        }

        // Begin: Cascade Dropdown For States
        public JsonResult fetchStates(int JsonCountryValueId)
        {
            var selStates = sharedManager.State
                .OrderBy(s => s.Name)
                .Where(s => s.CountryID == JsonCountryValueId)
                .Select(s => new { value = s.StateID, text = s.Name });
            return Json(new SelectList(selStates, "value", "text"));
        }

        // Begin: Cascade Dropdown For Cities
        public JsonResult fetchCities(int JsonStateValueId)
        {
            var selCities = sharedManager.City
                .OrderBy(c => c.Name)
                .Where(c => c.StateID == JsonStateValueId)
                .Select(c => new { value = c.CityID, text = c.Name });
            return Json(new SelectList(selCities, "value", "text"));
        }

        // Begin: Cascade Dropdown For Assemblies
        public JsonResult fetchAssemblies(int JsonCityValueId)
        {
            var selAssemblies = sharedManager.Location
                .OrderBy(c => c.Name)
                .Where(c => c.CityID == JsonCityValueId)
                .Select(c => new { value = c.Id, text = c.Name });
            return Json(new SelectList(selAssemblies, "value", "text"));
        }

        // Begin: Cascade Dropdown For Pincode
        public JsonResult fetchPincodes(int JsonAssemblyValueId)
        {
            var selPincodes = sharedManager.Pincode
                .OrderBy(c => c.PincodeNumber)
                .Where(c => c.LocationId == JsonAssemblyValueId)
                .Select(c => new { value = c.PincodeID, text = c.PincodeNumber });
            return Json(new SelectList(selPincodes, "value", "text"));
        }

        // Begin: Cascade Dropdown For Locality
        public JsonResult fetchLocalities(int JsonPincodeValueId)
        {
            var selLocalities = sharedManager.Area
                .OrderBy(c => c.Name)
                .Where(c => c.PincodeID == JsonPincodeValueId)
                .Select(c => new { value = c.Id, text = c.Name });
            return Json(new SelectList(selLocalities, "value", "text"));
        }
    }
}
