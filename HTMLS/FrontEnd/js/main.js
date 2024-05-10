(function ($) {
    $('.navbar-brand-btn').click(function() {
        $(this).toggleClass('active');
        $('.mim-HomeSideMenu').toggleClass('show');     
    });
    
    $('.navbar-brand-offer-menu .navbar-brand-offer-btn').click(function() {
        $(this).toggleClass('active');
        $(this).closest('.category-list').next('.brand-category-list').find('.navbar-brand-offer-list').toggleClass('show'); 
    });

    $('.navbar-brand-offer-list .nav-link').click(function() {
        $('.navbar-brand-offer-menu .navbar-brand-offer-btn').click();
    });

    $('.mim-box-list a').click(function() {
        $('.navbar-brand-btn').click();
    });
})(window.jQuery); 

$(document).ready(function () {    
    $('#HideNumber').hide();
    $('#contactDetails').hide();

    $("#ViewNumber").click(function () {
        $("#ViewNumber").hide();
        $("#HideNumber").show();
        $(this).parent('.social-details').next('#contactDetails ').show();
    });

    $("#HideNumber").click(function () {
        $("#ViewNumber").show();
        $("#HideNumber").hide();
        $(this).parent('.social-details').next('#contactDetails ').hide();
    });  
    $(".gander").select2({
        placeholder: "Gander",
        allowClear: true,
    });
    $(".turnover").select2({
        placeholder: "Turnover",
        allowClear: true,
    });
    $(".natureofbusiness").select2({
        placeholder: "Nature of Business",
        allowClear: true,
    });
    $(".designation").select2({
        placeholder: "Designation",
        allowClear: true,
    });
    $(".country").select2({
        placeholder: "Choose one",
        allowClear: true,
    });
    $(".state").select2({
        placeholder: "Choose one",
        allowClear: true,
    });
    $(".city").select2({
        placeholder: "Choose one",
        allowClear: true,
    });
    $(".pincode").select2({
        placeholder: "Choose one",
        allowClear: true,
    });  

    $(".locality").select2({
        placeholder: "Choose one",
        allowClear: true,
    });

    $(".assembly").select2({
        placeholder: "Designation",
        allowClear: true,
    });
    $(".fcategory").select2({
        placeholder: "First Category",
        allowClear: true,
    });
    $(".scategory").select2({
        placeholder: "Second Category",
        allowClear: true,
    });

    $("#v-pills-tabContent form").on('submit',function(e) {
        console.log('test');
        e.preventDefault();
        var li_count = $('#v-pills-tab-listing a').length;
        var current_active = $('.# a.active').index();
  
        if(current_active<li_count){
          $('#v-pills-tab-listing a.active').next('a.nav-link').attr('data-toggle','pill').tab('show');
        }else{
          alert('Last Step');
        }
    });

    var selectAllItems = "#select-all";
    var checkboxItem = ":checkbox";

    $(selectAllItems).click(function() {
      
      if (this.checked) {
        $(checkboxItem).each(function() {
          this.checked = true;
        });
      } else {
        $(checkboxItem).each(function() {
          this.checked = false;
        });
      }
      
    });   
});


     
// ---------------- Owl Carousel --------------


$('.owl-carousel-1').owlCarousel({
    rtl: false,
    loop: true,
    margin: 10,
    nav: false,
    responsiveClass:true,
    responsive: {
        0: {
            items: 1
        },
        600: {
            items: 1
        },
        1000: {
            items: 2
        }
    }
});

$('.owl-carousel-2').owlCarousel({
    rtl: false,
    loop: true,
    margin: 15,
    nav: false,
    responsiveClass:true,
    responsive: {
        0: {
            items: 1
        },
        600: {
            items: 2
        },
        1000: {
            items: 4
        }
    }
});

$('.owl-carousel').owlCarousel({
    rtl: false,
    loop: true,
    margin: 10,
    nav: false,
    responsiveClass:true,
    responsive: {
        0: {
            items: 2
        },
        600: {
            items: 3
        },
        1000: {
            items: 4
        }
    }
});





// --------------------- Image Upload ---------------

$(document).ready(function() {
    $('#logo_img_btn').click(function() {
        $('#logo_img_btn').removeClass('select');
        $('#owner_img_btn').addClass('select');
        $('#gallery_img_btn').addClass('select');
        $('#banner_img_btn').addClass('select');
        $('#certificate_img_btn').addClass('select');
        $('#client_logo_img_btn').addClass('select');

        $('#logo_upload_sec').removeClass('hide');
        $('#owner_upload_sec').addClass('hide');
        $('#gallery_upload_sec').addClass('hide');
        $('#banner_img_sec').addClass('hide');
        $('#certificate_img_sec').addClass('hide');
        $('#client_logo_img_sec').addClass('hide');
    });

    $('#owner_img_btn').click(function() {
        $('#logo_img_btn').addClass('select');
        $('#owner_img_btn').removeClass('select');
        $('#gallery_img_btn').addClass('select');
        $('#banner_img_btn').addClass('select');
        $('#certificate_img_btn').addClass('select');
        $('#client_logo_img_btn').addClass('select');

        $('#logo_upload_sec').addClass('hide');
        $('#owner_upload_sec').removeClass('hide');
        $('#gallery_upload_sec').addClass('hide');
        $('#banner_img_sec').addClass('hide');
        $('#certificate_img_sec').addClass('hide');
        $('#client_logo_img_sec').addClass('hide');
    });

    $('#gallery_img_btn').click(function() {
        $('#logo_img_btn').addClass('select');
        $('#owner_img_btn').addClass('select');
        $('#gallery_img_btn').removeClass('select');
        $('#banner_img_btn').addClass('select');
        $('#certificate_img_btn').addClass('select');
        $('#client_logo_img_btn').addClass('select');

        $('#logo_upload_sec').addClass('hide');
        $('#owner_upload_sec').addClass('hide');
        $('#gallery_upload_sec').removeClass('hide');
        $('#banner_img_sec').addClass('hide');
        $('#certificate_img_sec').addClass('hide');
        $('#client_logo_img_sec').addClass('hide');
    });
    $('#banner_img_btn').click(function() {
        $('#logo_img_btn').addClass('select');
        $('#owner_img_btn').addClass('select');
        $('#gallery_img_btn').addClass('select');
        $('#banner_img_btn').removeClass('select');
        $('#certificate_img_btn').addClass('select');
        $('#client_logo_img_btn').addClass('select');

        $('#logo_upload_sec').addClass('hide');
        $('#owner_upload_sec').addClass('hide');
        $('#gallery_upload_sec').addClass('hide');
        $('#banner_img_sec').removeClass('hide');
        $('#certificate_img_sec').addClass('hide');
        $('#client_logo_img_sec').addClass('hide');
    });
    $('#certificate_img_btn').click(function() {
        $('#logo_img_btn').addClass('select');
        $('#owner_img_btn').addClass('select');
        $('#gallery_img_btn').addClass('select');
        $('#banner_img_btn').addClass('select');
        $('#certificate_img_btn').removeClass('select');
        $('#client_logo_img_btn').addClass('select');

        $('#logo_upload_sec').addClass('hide');
        $('#owner_upload_sec').addClass('hide');
        $('#banner_img_sec').addClass('hide');
        $('#gallery_upload_sec').addClass('hide');
        $('#certificate_img_sec').removeClass('hide');
        $('#client_logo_img_sec').addClass('hide');
    });
    $('#client_logo_img_btn').click(function() {
        $('#logo_img_btn').addClass('select');
        $('#owner_img_btn').addClass('select');
        $('#gallery_img_btn').addClass('select');
        $('#banner_img_btn').addClass('select');
        $('#certificate_img_btn').addClass('select');
        $('#client_logo_img_btn').removeClass('select');

        $('#logo_upload_sec').addClass('hide');
        $('#owner_upload_sec').addClass('hide');
        $('#banner_img_sec').addClass('hide');
        $('#certificate_img_sec').addClass('hide');
        $('#gallery_upload_sec').addClass('hide');
        $('#client_logo_img_sec').removeClass('hide');
    });
});


// product details slider active
$('.product-large-slider').slick({
    fade: true,
    arrows: false,
    speed: 1000,
    asNavFor: '.pro-nav'
});


// product details slider nav active
$('.pro-nav').slick({
    slidesToShow: 3,
    asNavFor: '.product-large-slider',
    centerMode: true,
    speed: 1000,
    centerPadding: 0,
    focusOnSelect: true,
    prevArrow: '<button type="button" class="slick-prev"><i class="lnr lnr-chevron-left"></i></button>',
    nextArrow: '<button type="button" class="slick-next"><i class="lnr lnr-chevron-right"></i></button>',
    responsive: [{
        breakpoint: 576,
        settings: {
            slidesToShow: 3,
        }
    }]
});


$(document).ready(function() {
    $('#bookmark').click(function() {
        $('#v-pills-dashboard').removeClass('active');
        $('#v-pills-dashboard').removeClass('show');
        $('#v-pills-dashboard-tab').removeClass('active');
        $('#v-pill-bookmark').addClass('active');
        $('#v-pill-bookmark').addClass('show');
    });

    $('#like').click(function() {
        $('#v-pills-dashboard').removeClass('active');
        $('#v-pills-dashboard').removeClass('show');
        $('#v-pills-dashboard-tab').removeClass('active');
        $('#v-pill-like').addClass('active');
        $('#v-pill-like').addClass('show');
    });

    $('#review').click(function() {
        $('#v-pills-dashboard').removeClass('active');
        $('#v-pills-dashboard').removeClass('show');
        $('#v-pills-dashboard-tab').removeClass('active');
        $('#v-pill-review').addClass('active');
        $('#v-pill-review').addClass('show');
    });

    $('#subscribe').click(function() {
        $('#v-pills-dashboard').removeClass('active');
        $('#v-pills-dashboard').removeClass('show');
        $('#v-pills-dashboard-tab').removeClass('active');
        $('#v-pill-subscribe').addClass('active');
        $('#v-pill-subscribe').addClass('show');
    });
});


function validateNumber(e) {

    const pattern = /^[0-9]$/;

    return pattern.test(e.key)

}



//  -------------- Multistep Form ----------------

var currentTab = 0; // Current tab is set to be the first tab (0)
showTab(currentTab); // Display the current tab

function showTab(n) {
  // This function will display the specified tab of the form...
  var x = document.getElementsByClassName("tab");
  x[n].style.display = "block";
  //... and fix the Previous/Next buttons:
  if (n == 0) {
    document.getElementById("prevBtn").style.display = "none";
  } else {
    document.getElementById("prevBtn").style.display = "inline";
  }
  if (n == (x.length - 1)) {
    document.getElementById("nextBtn").innerHTML = "Submit";
  } else {
    document.getElementById("nextBtn").innerHTML = "Next";
  }
  //... and run a function that will display the correct step indicator:
  fixStepIndicator(n)
}

function nextPrev(n) {
  // This function will figure out which tab to display
  var x = document.getElementsByClassName("tab");
  // Exit the function if any field in the current tab is invalid:
//   if (n == 1 && !validateForm()) return false;
  // Hide the current tab:
  x[currentTab].style.display = "none";
  // Increase or decrease the current tab by 1:
  currentTab = currentTab + n;
  // if you have reached the end of the form...
  if (currentTab >= x.length) {
    // ... the form gets submitted:
    document.getElementById("regForm").submit();
    return false;
  }
  // Otherwise, display the correct tab:
  showTab(currentTab);
}



function fixStepIndicator(n) {
  // This function removes the "active" class of all steps...
  var i, x = document.getElementsByClassName("step");
  for (i = 0; i < x.length; i++) {
    x[i].className = x[i].className.replace(" active", "");
  }
  //... and adds the "active" class on the current step:
  x[n].className += " active";
}




// Image zoom effect
$('.img-zoom').zoom();



