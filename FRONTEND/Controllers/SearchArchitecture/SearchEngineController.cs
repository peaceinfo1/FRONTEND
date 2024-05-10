using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BAL.Addresses;
using BAL.Listings;
using BAL.Search;
using BAL.Services.Contracts;
using BOL.VIEWMODELS;
using DAL.CATEGORIES;
using DAL.LISTING;
using DAL.SHARED;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using ReflectionIT.Mvc.Paging;

namespace FRONTEND.Controllers.SearchArchitecture
{
    public class SearchEngineController : Controller
    {
        private readonly ListingDbContext listingContext;
        private readonly CategoriesDbContext categoryContext;
        private readonly SharedDbContext sharedContext;
        private readonly IAddresses addressesRepository;
        private readonly IListingManager listingManager;
        private readonly ISearch searchService;
        private readonly IUserService _userService;

        public SearchEngineController(ListingDbContext listingContext, CategoriesDbContext categoryContext, 
            IAddresses addressesRepository, IListingManager listingManager, SharedDbContext sharedContext, 
            ISearch searchService, IUserService userService)
        {
            this.listingContext = listingContext;
            this.categoryContext = categoryContext;
            this.addressesRepository = addressesRepository;
            this.listingManager = listingManager;
            this.sharedContext = sharedContext;
            this.searchService = searchService;
            this._userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> FetchKeywords(string term)
        {
            // Begin: First Categories
            IList<SearchResultViewModel> firstCategoriesResult = new List<SearchResultViewModel>();
            var firstCategories = await categoryContext.FirstCategory.Where(i => i.SearchKeywordName.Contains(term)).OrderBy(i => i.SearchCount).Take(10).ToListAsync();
            foreach (var item in firstCategories)
            {
                var firstCat = new SearchResultViewModel()
                {
                    label = "<li><div style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>" + item.SearchKeywordName + "</div></li>",
                    value = $"({item.SearchKeywordName})-UrlStart-{item.URL}-UrlEnd-TermStart-CI-TermEnd-IdStart-{item.FirstCategoryID}-IdEnd"
                };
                firstCategoriesResult.Add(firstCat);
            }

            var finalFirstCategoriesResult = (from i in firstCategoriesResult
                                              where i.label != null
                                              select new { i.label, i.value });
            // End:

            // Begin: Second Categories
            IList<SearchResultViewModel> secondCategoriesResult = new List<SearchResultViewModel>();
            var secondCategories = await categoryContext.SecondCategory.Where(i => i.SearchKeywordName.Contains(term)).OrderBy(i => i.SearchCount).Take(10).ToListAsync();
            foreach (var item in secondCategories)
            {
                var secondCat = new SearchResultViewModel()
                {
                    label = "<li><div style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>" + item.SearchKeywordName + "</div></li>",
                    value = $"({item.SearchKeywordName})-UrlStart-{item.URL}-UrlEnd-TermStart-CII-TermEnd-IdStart-{item.SecondCategoryID}-IdEnd"
                };
                secondCategoriesResult.Add(secondCat);
            }

            var finalSecondCategoriesResult = (from i in secondCategoriesResult
                                               where i.label != null
                                               select new { i.label, i.value });
            // End:

            // Begin: Third Categories
            IList<SearchResultViewModel> thirdCategoriesResult = new List<SearchResultViewModel>();
            var thirdCategories = await categoryContext.ThirdCategory.Where(i => i.SearchKeywordName.Contains(term)).OrderBy(i => i.SearchCount).Take(10).ToListAsync();
            foreach (var item in thirdCategories)
            {
                var thirdCat = new SearchResultViewModel()
                {
                    label = "<li><div style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>" + item.SearchKeywordName + "</div></li>",
                    value = $"({item.SearchKeywordName})-UrlStart-{item.URL}-UrlEnd-TermStart-CIII-TermEnd-IdStart-{item.FirstCategoryID}-IdEnd"
                };

                thirdCategoriesResult.Add(thirdCat);
            }

            var finalThirdCategoriesResult = (from i in thirdCategoriesResult
                                              where i.label != null
                                              select new { i.label, i.value });
            // End:

            // Begin: Fourth Categories
            IList<SearchResultViewModel> fourthCategoriesResult = new List<SearchResultViewModel>();
            var fourthCategories = await categoryContext.FourthCategory.Where(i => i.SearchKeywordName.Contains(term)).OrderBy(i => i.SearchCount).Take(10).ToListAsync();
            foreach (var item in fourthCategories)
            {
                var fourthCat = new SearchResultViewModel()
                {
                    label = "<li><div style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>" + item.SearchKeywordName + "</div></li>",
                    value = $"({item.SearchKeywordName})-UrlStart-{item.URL}-UrlEnd-TermStart-CIV-TermEnd-IdStart-{item.FirstCategoryID}-IdEnd"
                };

                fourthCategoriesResult.Add(fourthCat);
            }

            var finalFourthCategoriesResult = (from i in fourthCategoriesResult
                                               where i.label != null
                                               select new { i.label, i.value });
            // End:

            // Begin: Fifth Categories
            IList<SearchResultViewModel> fifthCategoriesResult = new List<SearchResultViewModel>();
            var fifthCategories = await categoryContext.FifthCategory.Where(i => i.SearchKeywordName.Contains(term)).OrderBy(i => i.SearchCount).Take(10).ToListAsync();
            foreach (var item in fifthCategories)
            {
                var fifthCat = new SearchResultViewModel()
                {
                    label = "<li><div style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>" + item.SearchKeywordName + "</div></li>",
                    value = $"({item.SearchKeywordName})-UrlStart-{item.URL}-UrlEnd-TermStart-CV-TermEnd-IdStart-{item.FirstCategoryID}-IdEnd"
                };

                fifthCategoriesResult.Add(fifthCat);
            }

            var finalFifthCategoriesResult = (from i in fifthCategoriesResult
                                              where i.label != null
                                              select new { i.label, i.value });
            // End:

            // Begin: Sixth Categories
            IList<SearchResultViewModel> sixthCategoriesResult = new List<SearchResultViewModel>();
            var sixthCategories = await categoryContext.SixthCategory.Where(i => i.SearchKeywordName.Contains(term)).OrderBy(i => i.SearchCount).Take(10).ToListAsync();
            foreach (var item in sixthCategories)
            {
                var sixthCat = new SearchResultViewModel()
                {
                    label = "<li><div style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>" + item.SearchKeywordName + "</div></li>",
                    value = $"({item.SearchKeywordName})-UrlStart-{item.URL}-UrlEnd-TermStart-CVI-TermEnd-IdStart-{item.FirstCategoryID}-IdEnd"
                };
                sixthCategoriesResult.Add(sixthCat);
            }

            var finalSixthCategoriesResult = (from i in sixthCategoriesResult
                                              where i.label != null
                                              select new { i.label, i.value });
            // End:

            // Shafi: Keyword First Category
            IList<SearchResultViewModel> firstCategoryKeywordResult = new List<SearchResultViewModel>();
            var firstCategoryKeyword = await categoryContext.KeywordFirstCategory.Where(i => i.Keyword.Contains(term)).OrderBy(i => i.SearchCount).Take(10).ToListAsync();
            foreach (var item in firstCategoryKeyword)
            {
                var firstCatKeyword = new SearchResultViewModel()
                {
                    label = "<li><div style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>" + item.Keyword + "</div></li>",
                    value = $"({item.Keyword})-UrlStart-{item.URL}-UrlEnd-TermStart-KI-TermEnd-IdStart-{item.KeywordFirstCategoryID}-IdEnd"
                };
                firstCategoryKeywordResult.Add(firstCatKeyword);
            }

            var finalFirstCategoryKeywordResult = (from i in firstCategoryKeywordResult
                                                   where i.label != null
                                                   select new { i.label, i.value });
            // End:

            // Shafi: Keyword Second Category
            IList<SearchResultViewModel> secondCategoryKeywordResult = new List<SearchResultViewModel>();
            var secondCategoryKeyword = await categoryContext.KeywordSecondCategory.Where(i => i.Keyword.Contains(term)).OrderBy(i => i.SearchCount).Take(10).ToListAsync();
            foreach (var item in secondCategoryKeyword)
            {
                var secondCatKeyword = new SearchResultViewModel()
                {
                    label = "<li><div style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>" + item.Keyword + "</div></li>",
                    value = $"({item.Keyword})-UrlStart-{item.URL}-UrlEnd-TermStart-KII-TermEnd-IdStart-{item.KeywordSecondCategoryID}-IdEnd"
                };
                secondCategoryKeywordResult.Add(secondCatKeyword);
            }

            var finalSecondCategoryKeywordResult = (from i in secondCategoryKeywordResult
                                                    where i.label != null
                                                    select new { i.label, i.value });
            // End:

            // Shafi: Keyword Third Category
            IList<SearchResultViewModel> thirdCategoryKeywordResult = new List<SearchResultViewModel>();
            var thirdCategoryKeyword = await categoryContext.KeywordThirdCategory.Where(i => i.Keyword.Contains(term)).OrderBy(i => i.SearchCount).Take(10).ToListAsync();
            foreach (var item in thirdCategoryKeyword)
            {
                var thirdCatKeyword = new SearchResultViewModel()
                {
                    label = "<li><div style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>" + item.Keyword + "</div></li>",
                    value = $"({item.Keyword})-UrlStart-{item.URL}-UrlEnd-TermStart-KIII-TermEnd-IdStart-{item.KeywordThirdCategoryID}-IdEnd"
                };
                thirdCategoryKeywordResult.Add(thirdCatKeyword);
            }

            var finalThirdCategoryKeywordResult = (from i in thirdCategoryKeywordResult
                                                   where i.label != null
                                                   select new { i.label, i.value });
            // End:

            // Shafi: Keyword Fourth Category
            IList<SearchResultViewModel> fourthCategoryKeywordResult = new List<SearchResultViewModel>();
            var fourthCategoryKeyword = await categoryContext.KeywordFourthCategory.Where(i => i.Keyword.Contains(term)).OrderBy(i => i.SearchCount).Take(10).ToListAsync();
            foreach (var item in fourthCategoryKeyword)
            {
                var fourthCatKeyword = new SearchResultViewModel()
                {
                    label = "<li><div style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>" + item.Keyword + "</div></li>",
                    value = $"({item.Keyword})-UrlStart-{item.URL}-UrlEnd-TermStart-KIV-TermEnd-IdStart-{item.KeywordFourthCategoryID}-IdEnd"
                };
                fourthCategoryKeywordResult.Add(fourthCatKeyword);
            }

            var finalFourthCategoryKeywordResult = (from i in fourthCategoryKeywordResult
                                                    where i.label != null
                                                    select new { i.label, i.value });
            // End:

            // Shafi: Keyword Fifth Category
            IList<SearchResultViewModel> fifthCategoryKeywordResult = new List<SearchResultViewModel>();
            var fifthCategoryKeyword = await categoryContext.KeywordFifthCategory.Where(i => i.Keyword.Contains(term)).OrderBy(i => i.SearchCount).Take(10).ToListAsync();
            foreach (var item in fifthCategoryKeyword)
            {
                var fifthCatKeyword = new SearchResultViewModel()
                {
                    label = "<li><div style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>" + item.Keyword + "</div></li>",
                    value = $"({item.Keyword})-UrlStart-{item.URL}-UrlEnd-TermStart-KV-TermEnd-IdStart-{item.KeywordFifthCategoryID}-IdEnd"
                };
                fifthCategoryKeywordResult.Add(fifthCatKeyword);
            }

            var finalFifthCategoryKeywordResult = (from i in fifthCategoryKeywordResult
                                                   where i.label != null
                                                   select new { i.label, i.value });
            // End:

            // Shafi: Keyword Sixth Category
            IList<SearchResultViewModel> sixthCategoryKeywordResult = new List<SearchResultViewModel>();
            var sixthCategoryKeyword = await categoryContext.KeywordSixthCategory.Where(i => i.Keyword.Contains(term)).OrderBy(i => i.SearchCount).Take(10).ToListAsync();
            foreach (var item in sixthCategoryKeyword)
            {
                var sixthCatKeyword = new SearchResultViewModel()
                {
                    label = "<li><div style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>" + item.Keyword + "</div></li>",
                    value = $"({item.Keyword})-UrlStart-{item.URL}-UrlEnd-TermStart-KVI-TermEnd-IdStart-{item.KeywordSixthCategoryID}-IdEnd"
                };
                sixthCategoryKeywordResult.Add(sixthCatKeyword);
            }

            var finalSixthCategoryKeywordResult = (from i in sixthCategoryKeywordResult
                                                   where i.label != null
                                                   select new { i.label, i.value });
            // End:

            // Begin: Companies
            IList<SearchResultViewModel> companyResult = new List<SearchResultViewModel>();
            var companies = await listingContext.Listing.Where(i => i.CompanyName.Contains(term)).Take(10).ToListAsync();

            foreach (var item in companies)
            {
                if (listingManager.CheckIfListingFullfillFreeListingCrieteria(item.ListingID) == true)
                {
                    var address = await listingContext.Address.Where(i => i.ListingID == item.ListingID).FirstOrDefaultAsync();
                    var locality = await addressesRepository.LocalityDetailsAsync(address.LocalityID);
                    var station = await addressesRepository.StationDetailsAsync(address.AssemblyID);
                    var city = await addressesRepository.CityDetailsAsync(address.City);
                    var state = await addressesRepository.StateDetailsAsync(address.StateID);
                    var country = await addressesRepository.CountryDetailsAsync(address.CountryID);

                    var name = new SearchResultViewModel()
                    {
                        label = "<li><div id='elementSearchResult' style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>" + item.CompanyName + "</div><div style='font-family: Arial; margin-top:-3px; margin-left:20px; font-size: 11px;'>" + locality.Name + " " + station.Name + " " + city.Name + " " + state.Name + " " + country.Name + "</div></li>",
                        value = $"({item.CompanyName})-UrlStart-{item.ListingURL}-UrlEnd-TermStart-COM-TermEnd-IdStart-{item.ListingID}-IdEnd"
                    };

                    companyResult.Add(name);
                }
            }
            var finalCompanyResult = (from i in companyResult
                                      where i.label != null
                                      select new { i.label, i.value });
            // End:

            // Begin: Person
            IList<SearchResultViewModel> contactPersonResult = new List<SearchResultViewModel>();
            var contactPerson = await listingContext.Listing.Where(i => i.Name.Contains(term)).Take(10).ToListAsync();

            foreach (var item in contactPerson)
            {
                if (listingManager.CheckIfListingFullfillFreeListingCrieteria(item.ListingID) == true)
                {
                    var address = await listingContext.Address.Where(i => i.ListingID == item.ListingID).FirstOrDefaultAsync();
                    var locality = await addressesRepository.LocalityDetailsAsync(address.LocalityID);
                    var station = await addressesRepository.StationDetailsAsync(address.AssemblyID);
                    var city = await addressesRepository.CityDetailsAsync(address.City);
                    var state = await addressesRepository.StateDetailsAsync(address.StateID);
                    var country = await addressesRepository.CountryDetailsAsync(address.CountryID);

                    var contact = new SearchResultViewModel()
                    {
                        label = $"<li><div id='elementSearchResult' style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>{item.CompanyName}</div><div style='font-family: Arial; margin-top:-3px; margin-left:20px; font-size: 11px;'>{item.Name} <strong>Location: </strong>{locality.Name} {station.Name} {city.Name} {state.Name} {country.Name}</div></li>",
                        value = $"({item.Name})-UrlStart-{item.ListingURL}-UrlEnd-TermStart-PER-TermEnd-IdStart-{item.ListingID}-IdEnd"
                    };

                    contactPersonResult.Add(contact);
                }
            }
            var finalContactPersonResult = (from i in contactPersonResult
                                            where i.label != null
                                            select new { i.label, i.value });
            // End:

            // Shafi: Mobile
            IList<SearchResultViewModel> mobileResult = new List<SearchResultViewModel>();
            var mobiles = await listingContext.Communication.Where(i => i.Mobile.Contains(term)).Take(10).ToListAsync();
            foreach (var item in mobiles)
            {
                if (listingManager.CheckIfListingFullfillFreeListingCrieteria(item.ListingID) == true)
                {

                    var listing = await listingManager.GetListingDetailsAsync(item.ListingID);
                    var address = await listingContext.Address.Where(i => i.ListingID == item.ListingID).FirstOrDefaultAsync();
                    var station = await addressesRepository.StationDetailsAsync(address.AssemblyID);
                    var city = await addressesRepository.CityDetailsAsync(address.City);

                    var number = new SearchResultViewModel()
                    {
                        label = $"<li><div id='elementSearchResult' style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>{listing.CompanyName}</div><div style='font-family: Arial; margin-top:-3px; margin-left:20px; font-size: 11px;'>{item.Mobile.Substring(0, 3) }****{ item.Mobile.Substring(item.Mobile.Length - 3) } <strong>Location:</strong> {station.Name} {city.Name}</div></li>",
                        value = $"({item.Mobile})-UrlStart-{listing.ListingURL}-UrlEnd-TermStart-MOB-TermEnd-IdStart-{item.ListingID}-IdEnd"
                    };

                    mobileResult.Add(number);
                }
            }
            var finalMobileResult = (from i in mobileResult
                                     where i.label != null
                                     select new { i.label, i.value });
            // End:

            // Shafi: Telephone
            //IList<SearchResultViewModel> telephone1Result = new List<SearchResultViewModel>();
            //var telephone1List = await listingContext.Communication.Where(i => i.Telephone.Contains(term)).Take(10).ToListAsync();
            //foreach (var item in telephone1List)
            //{
            //    if (listingManager.CheckIfListingFullfillFreeListingCrieteria(item.ListingID) == true)
            //    {

            //        var listing = await listingManager.GetListingDetailsAsync(item.ListingID);


            //        var address = await listingContext.Address.Where(i => i.ListingID == item.ListingID).FirstOrDefaultAsync();
            //        if(address != null)
            //        {
            //            var station = await addressesRepository.StationDetailsAsync(address.AssemblyID);
            //            var city = await addressesRepository.CityDetailsAsync(address.City);

            //            var number = new SearchResultViewModel()
            //            {
            //                label = $"<li><div id='elementSearchResult' style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>{listing.CompanyName}</div><div style='font-family: Arial; margin-top:-3px; margin-left:20px; font-size: 11px;'>{item.Telephone.Substring(0, 3) }****{ item.Telephone.Substring(item.Mobile.Length - 3) } <strong>Location:</strong> {station.Name} {city.Name}</div></li>",
            //                value = $"({item.Telephone})-UrlStart-{listing.ListingURL}-UrlEnd-TermStart-TELI-TermEnd-IdStart-{item.ListingID}-IdEnd"
            //            };
            //            telephone1Result.Add(number);
            //        }
            //        else
            //        {
            //            var number = new SearchResultViewModel()
            //            {
            //                label = $"<li><div id='elementSearchResult' style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>{listing.CompanyName}</div><div style='font-family: Arial; margin-top:-3px; margin-left:20px; font-size: 11px;'>{item.Telephone.Substring(0, 3) }****{ item.Telephone.Substring(item.Telephone.Length - 3) }</div></li>",
            //                value = $"({item.Telephone})-UrlStart-{listing.ListingURL}-UrlEnd-TermStart-TELI-TermEnd-IdStart-{item.ListingID}-IdEnd"
            //            };
            //            telephone1Result.Add(number);
            //        }
            //    }
            //}
            //var finalTelephone1Result = (from i in telephone1Result
            //                             where i.label != null
            //                         select new { i.label, i.value });
            // End:

            // Shafi: Whatsapp
            IList<SearchResultViewModel> whatsappResult = new List<SearchResultViewModel>();
            var whatsappNumbers = await listingContext.Communication.Where(i => i.Whatsapp.Contains(term)).Take(10).ToListAsync();
            foreach (var item in whatsappNumbers)
            {
                if (listingManager.CheckIfListingFullfillFreeListingCrieteria(item.ListingID) == true)
                {
                    var listing = await listingManager.GetListingDetailsAsync(item.ListingID);
                    var address = await listingContext.Address.Where(i => i.ListingID == item.ListingID).FirstOrDefaultAsync();
                    var station = await addressesRepository.StationDetailsAsync(address.AssemblyID);
                    var city = await addressesRepository.CityDetailsAsync(address.City);
                        
                    var whatsapp = new SearchResultViewModel()
                    {
                        label = $"<li><div id='elementSearchResult' style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>{listing.CompanyName}</div><div style='font-family: Arial; margin-top:-3px; margin-left:20px; font-size: 11px;'>{item.Whatsapp.Substring(0, 3) }****{ item.Mobile.Substring(item.Whatsapp.Length - 3) } <strong>Location:</strong> {station.Name} {city.Name}</div></li>",
                        value = $"({item.Whatsapp})-UrlStart-{listing.ListingURL}-UrlEnd-TermStart-WHAT-TermEnd-IdStart-{item.ListingID}-IdEnd"
                    };

                    whatsappResult.Add(whatsapp);
                }
            }
            var finalWhatsappResult = (from i in whatsappResult
                                       where i.label != null
                                       select new { i.label, i.value });
            // End:

            // Shafi: Whatsapp
            IList<SearchResultViewModel> websiteResult = new List<SearchResultViewModel>();
            var websiteList = await listingContext.Communication.Where(i => i.Website.Contains(term)).Take(10).ToListAsync();
            foreach (var item in websiteList)
            {
                if (listingManager.CheckIfListingFullfillFreeListingCrieteria(item.ListingID) == true)
                {
                    var listing = await listingManager.GetListingDetailsAsync(item.ListingID);
                    var address = await listingContext.Address.Where(i => i.ListingID == item.ListingID).FirstOrDefaultAsync();

                    if(address != null)
                    {
                        var station = await addressesRepository.StationDetailsAsync(address.AssemblyID);
                        var city = await addressesRepository.CityDetailsAsync(address.City);

                        if (listingManager.CheckIfListingFullfillFreeListingCrieteria(item.ListingID) == true)
                        {
                            var website = new SearchResultViewModel()
                            {
                                label = $"<li><div id='elementSearchResult' style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>{listing.CompanyName}</div><div style='font-family: Arial; margin-top:-3px; margin-left:20px; font-size: 11px;'>{item.Website} <strong>Location:</strong> {station.Name} {city.Name}</div></li>",
                                value = $"({item.Website})-UrlStart-{listing.ListingURL}-UrlEnd-TermStart-WEB-TermEnd-IdStart-{item.ListingID}-IdEnd"
                            };

                            websiteResult.Add(website);
                        }
                    }
                    else
                    {
                        if (listingManager.CheckIfListingFullfillFreeListingCrieteria(item.ListingID) == true)
                        {
                            var website = new SearchResultViewModel()
                            {
                                label = $"<li><div id='elementSearchResult' style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>{listing.CompanyName}</div><div style='font-family: Arial; margin-top:-3px; margin-left:20px; font-size: 11px;'>{item.Website}</div></li>",
                                value = $"({item.Website})-UrlStart-{listing.ListingURL}-UrlEnd-TermStart-WEB-TermEnd-IdStart-{item.ListingID}-IdEnd"
                            };

                            websiteResult.Add(website);
                        }
                    }
                }
            }
            var finalWebsiteResult = (from i in websiteResult
                                      where i.label != null
                                      select new { i.label, i.value });
            // End:

            // Shafi: Toll Free
            IList<SearchResultViewModel> tollFreeResult = new List<SearchResultViewModel>();
            var tollFreeNumbers = await listingContext.Communication.Where(i => i.TollFree.Contains(term)).Take(10).ToListAsync();
            foreach (var item in tollFreeNumbers)
            {
                if (listingManager.CheckIfListingFullfillFreeListingCrieteria(item.ListingID) == true)
                {
                    var listing = await listingManager.GetListingDetailsAsync(item.ListingID);
                    var address = await listingContext.Address.Where(i => i.ListingID == item.ListingID).FirstOrDefaultAsync();
                    var station = await addressesRepository.StationDetailsAsync(address.AssemblyID);
                    var city = await addressesRepository.CityDetailsAsync(address.City);

                    var tollFree = new SearchResultViewModel()
                    {
                        label = $"<li><div id='elementSearchResult' style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>{listing.CompanyName}</div><div style='font-family: Arial; margin-top:-3px; margin-left:20px; font-size: 11px;'>{item.TollFree} <strong>Location:</strong> {station.Name} {city.Name}</div></li>",
                        value = $"({item.TollFree})-UrlStart-{listing.ListingURL}-UrlEnd-TermStart-TOL-TermEnd-IdStart-{item.ListingID}-IdEnd"
                    };

                    mobileResult.Add(tollFree);
                }
            }
            var finalTollFreeResult = (from i in tollFreeResult
                                       where i.label != null
                                       select new { i.label, i.value });
            // End:

            // Shafi: Country
            IList<SearchResultViewModel> countryResult = new List<SearchResultViewModel>();
            var countryList = await sharedContext.Country.Where(i => i.Name.Contains(term)).Take(10).ToListAsync();
            foreach (var item in countryList)
            {
                var country = new SearchResultViewModel()
                {
                    label = $"<li><div id='elementSearchResult' style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>{item.Name}</div></li>",
                    value = $"UrlStart-({item.Name})-UrlEnd-TermStart-CON-TermEnd-IdStart-{item.CountryID}-IdEnd"
                };

                countryResult.Add(country);
            }
            var finalCountryResult = (from i in countryResult
                                      where i.label != null
                                      select new { i.label, i.value });
            // End:

            // Shafi: State
            IList<SearchResultViewModel> stateResult = new List<SearchResultViewModel>();
            var stateList = await sharedContext.State.Where(i => i.Name.Contains(term)).Take(10).ToListAsync();
            foreach (var item in stateList)
            {
                var state = new SearchResultViewModel()
                {
                    label = $"<li><div id='elementSearchResult' style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>{item.Name}</div></li>",
                    value = $"UrlStart-({item.Name})-UrlEnd-TermStart-STA-TermEnd-IdStart-{item.StateID}-IdEnd"
                };

                stateResult.Add(state);
            }
            var finalStateResult = (from i in stateResult
                                    where i.label != null
                                    select new { i.label, i.value });
            // End:

            // Shafi: City
            IList<SearchResultViewModel> cityResult = new List<SearchResultViewModel>();
            var cityList = await sharedContext.City.Where(i => i.Name.Contains(term)).Take(10).ToListAsync();
            foreach (var item in cityList)
            {
                var city = new SearchResultViewModel()
                {
                    label = $"<li><div id='elementSearchResult' style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>{item.Name}</div></li>",
                    value = $"UrlStart-({item.Name})-UrlEnd-TermStart-CIT-TermEnd-IdStart-{item.CityID}-IdEnd"
                };

                cityResult.Add(city);
            }
            var finalCityResult = (from i in cityResult
                                   where i.label != null
                                   select new { i.label, i.value });
            // End:

            // Shafi: Assembly
            IList<SearchResultViewModel> assemblyResult = new List<SearchResultViewModel>();
            var assemblyList = await sharedContext.Location.Where(i => i.Name.Contains(term)).Take(10).ToListAsync();
            foreach (var item in assemblyList)
            {
                var assembly = new SearchResultViewModel()
                {
                    label = $"<li><div id='elementSearchResult' style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>{item.Name}</div></li>",
                    value = $"UrlStart-({item.Name})-UrlEnd-TermStart-ASB-TermEnd-IdStart-{item.CityID}-IdEnd"
                };

                assemblyResult.Add(assembly);
            }
            var finalAssemblyResult = (from i in assemblyResult
                                       where i.label != null
                                       select new { i.label, i.value });
            // End:

            // Shafi: Area
            IList<SearchResultViewModel> areaResult = new List<SearchResultViewModel>();
            var areaList = await sharedContext.Area.Where(i => i.Name.Contains(term)).Take(10).ToListAsync();
            foreach (var item in areaList)
            {
                var area = new SearchResultViewModel()
                {
                    label = $"<li><div id='elementSearchResult' style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>{item.Name}</div></li>",
                    value = $"UrlStart-({item.Name})-UrlEnd-TermStart-ARE-TermEnd-IdStart-{item.Id}-IdEnd"
                };

                areaResult.Add(area);
            }
            var finalAreaResult = (from i in areaResult
                                   where i.label != null
                                   select new { i.label, i.value });
            // End:

            // Shafi: Email
            IList<SearchResultViewModel> emailResult = new List<SearchResultViewModel>();
            if (term.Contains("@"))
            {
                var emails = await listingContext.Communication.Where(i => i.Email.Contains(term)).Take(10).ToListAsync();
                foreach (var item in emails)
                {
                    var listing = await listingManager.GetListingDetailsAsync(item.ListingID);
                    var address = await listingContext.Address.Where(i => i.ListingID == item.ListingID).FirstOrDefaultAsync();
                    var station = await addressesRepository.StationDetailsAsync(address.AssemblyID);
                    var city = await addressesRepository.CityDetailsAsync(address.City);

                    var email = new SearchResultViewModel()
                    {
                        label = $"<li><div id='elementSearchResult' style='margin-left:20px; text-decoration: none; color: #000000; font-family: Arial; font-size: 16px; font-weight: normal; cursor: pointer'>{listing.CompanyName}</div><div style='font-family: Arial; margin-top:-3px; margin-left:20px; font-size: 11px;'>{item.Email.Substring(0, 4) }*****@***** <strong>Location:</strong> {station.Name} {city.Name}</div></li>",
                        value = $"UrlStart-({item.Email})-UrlEnd-TermStart-EMA-TermEnd-IdStart-{item.ListingID}-IdEnd"
                    };

                    emailResult.Add(email);

                }

                var finalEmailResult = (from i in emailResult
                                        where i.label != null
                                        select new { i.label, i.value });

                var result = finalFirstCategoriesResult.Concat(finalSecondCategoriesResult).Concat(finalThirdCategoriesResult).Concat(finalFourthCategoriesResult).Concat(finalFifthCategoriesResult).Concat(finalSixthCategoriesResult).Concat(finalFirstCategoryKeywordResult).Concat(finalSecondCategoryKeywordResult).Concat(finalThirdCategoryKeywordResult).Concat(finalFourthCategoryKeywordResult).Concat(finalFifthCategoryKeywordResult).Concat(finalSixthCategoryKeywordResult).Concat(finalCompanyResult).Concat(finalContactPersonResult).Concat(finalMobileResult).Concat(finalWhatsappResult).Concat(finalTollFreeResult).OrderBy(i => i.label).Concat(finalEmailResult).Concat(finalCountryResult).Concat(finalStateResult).Concat(finalCityResult).Concat(finalAssemblyResult).Concat(finalAreaResult).Concat(finalWebsiteResult).OrderBy(i => i.label).Take(15);

                return Json(result);
            }
            else
            {
                var result = finalFirstCategoriesResult.Concat(finalSecondCategoriesResult).Concat(finalThirdCategoriesResult).Concat(finalFourthCategoriesResult).Concat(finalFifthCategoriesResult).Concat(finalSixthCategoriesResult).Concat(finalFirstCategoryKeywordResult).Concat(finalSecondCategoryKeywordResult).Concat(finalThirdCategoryKeywordResult).Concat(finalFourthCategoryKeywordResult).Concat(finalFifthCategoryKeywordResult).Concat(finalSixthCategoryKeywordResult).Concat(finalCompanyResult).Concat(finalContactPersonResult).Concat(finalMobileResult).Concat(finalWhatsappResult).Concat(finalTollFreeResult).Concat(finalCountryResult).Concat(finalStateResult).Concat(finalCityResult).Concat(finalAssemblyResult).Concat(finalAreaResult).Concat(finalWebsiteResult).OrderBy(i => i.label).Take(15);

                return Json(result);
            }
            // End:
        }

        // Shafi: Begin Search Result
        [HttpGet]
        [Route("/Search/{url}-{term}-{id}")]
        public async Task<IActionResult> SearchResult(string url, string term, int? id, string keyword, int pageindex = 1)
        {
            string city = searchService.GetCity();

            string userGuid = "";

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userService.GetUserByUserName(User.Identity.Name);
                userGuid = user.Id;
            }
            else
            {
                userGuid = "Anonymous";
            }

            if(term == null || id == null)
            {
                return Content(keyword);
            }

            if(term == "CI" || term == "CII" || term == "CIII" || term == "CIV" || term == "CV" || term == "CVI")
            {
                var result = await searchService.Category(term, id.Value, city);
                var jobId = BackgroundJob.Schedule(() => searchService.CreateSearchHistory(term, id.Value, userGuid), TimeSpan.FromSeconds(10));

                var impressionJobId = BackgroundJob.Schedule(() => searchService.IncrementListingImpression(result), TimeSpan.FromSeconds(10));

                var pageModel = PagingList.Create(result, 10, pageindex);

                return View(pageModel);
            }

            if (term == "KI" || term == "KII" || term == "KIII" || term == "KIV" || term == "KV" || term == "KVI")
            {
                var jobId = BackgroundJob.Schedule(() => searchService.CreateSearchHistory(term, id.Value, userGuid), TimeSpan.FromSeconds(10));
                var result = await searchService.Keyword(term, id.Value, city);
                return View(result);
            }

            if (term == "COM")
            {
                var jobId = BackgroundJob.Schedule(() => searchService.CreateSearchHistory(term, id.Value, userGuid), TimeSpan.FromSeconds(10));
                return Redirect($"/Listing/Index/{id}");
            }

            if (term == "PER")
            {
                var jobId = BackgroundJob.Schedule(() => searchService.CreateSearchHistory(term, id.Value, userGuid), TimeSpan.FromSeconds(10));
                return Redirect($"/Listing/Index/{id}");
            }

            if (term == "CON" || term == "STA" || term == "CIT" || term == "PIN" || term == "ARE" || term == "ASB")
            {
                var jobId = BackgroundJob.Schedule(() => searchService.CreateSearchHistory(term, id.Value, userGuid), TimeSpan.FromSeconds(10));
                var result = await searchService.Address(term, id.Value);
                return View(result);
            }

            if (term == "MOB" || term == "TELI" || term == "TELII" || term == "TOL" || term == "WEB" || term == "WHAT" || term == "EMA")
            {
                var jobId = BackgroundJob.Schedule(() => searchService.CreateSearchHistory(term, id.Value, userGuid), TimeSpan.FromSeconds(10));
                return Redirect($"/Listing/Index/{id}");
            }

            return View();
        }
    }
}
