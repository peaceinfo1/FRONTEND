using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BOL.LISTING;
using DAL.LISTING;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics.Eventing.Reader;
using Microsoft.AspNetCore.Http;
using DAL.SHARED;
using BAL.Listings;
using Microsoft.AspNetCore.Identity.UI.Services;
using BAL.Audit;
using System.Data;
using BAL.Services.Contracts;
using Slugify;
using BOL.DTO;

namespace FRONTEND.Areas.AjaxRequests.Controllers
{
    [Area("AjaxRequests")]
    public class ListingWizardController : ControllerBase
    {
        private readonly ListingDbContext listingContext;
        private readonly SharedDbContext sharedContext;
        private readonly IUserService _userService;
        private readonly IEmailSender emailSender;
        private readonly IHistoryAudit audit;
        private readonly IListingManager listingManager;

        public ListingWizardController(ListingDbContext listingContext, IUserService userService,
            SharedDbContext sharedContext, IEmailSender emailSender, IHistoryAudit audit, IListingManager listingManager)
        {
            this.listingContext = listingContext;
            this._userService = userService;
            this.sharedContext = sharedContext;
            this.emailSender = emailSender;
            this.audit = audit;
            this.listingManager = listingManager;
        }

        [HttpPost]
        public async Task<ReturnDTO> AddBusinessCategory(string businessCat)
        {
            try
            {
                ReturnDTO returnDTO = new ReturnDTO();

                if (businessCat != "")
                {
                    // Slugify
                    SlugHelper helper = new SlugHelper();
                    businessCat = helper.GenerateSlug(businessCat);

                    // Check If Business Category Exist
                    var businessCatExist = await listingContext.Listing
                        .AnyAsync(i => i.BusinessCategory.Contains(businessCat));

                    if (businessCatExist == true)
                    {
                        returnDTO.Success = false;
                        returnDTO.Message = $"Business category with name {businessCat} already exists, please choose a different name.";

                        // Return
                        return returnDTO;
                    }
                    else
                    {
                        returnDTO.Success = false;
                        returnDTO.Message = $"Business category with name {businessCat} created successfully.";

                        // Return
                        return returnDTO;
                    }
                }
                else
                {
                    returnDTO.Success = false;
                    returnDTO.Message = $"Business category name is empty, please provide a name.";

                    // Return
                    return returnDTO;
                }
            }
            catch(Exception exc)
            {
                ReturnDTO returnDTO = new ReturnDTO();

                returnDTO.Success = false;
                returnDTO.Message = exc.Message;

                // Return
                return returnDTO;
            }
        }
    }
}
