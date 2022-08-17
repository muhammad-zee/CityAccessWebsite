

function AppendToDrop(value, text, id) {

    var html = "<a href=" + value + ">" + text + "</a>";

    $("#" + id).append(html);
}


function ChangeExternalView(ButtonID) {
    var $ViewDiv = $("#ExternalView");
    var url = $("#" + ButtonID).data('url');

    $.get(url, function (data) {
        $ViewDiv.empty();
        $ViewDiv.append(data);
    });
}

function CreateBooking(partnerID) {

    $.ajax({
        url: '/Booking/Create',
        type: "Post",
        data: $('#createBooking').serialize(),
        success: function (result) {
            location.href = "/PartnerSite/BookingSuccessfull?partnerID=" + partnerID;

        },
        error: function (jqxhr, status, exception) {
            console.log('Exception:', exception);
        }
    });

}



function openNav() {

    if ($("#mySideNav").is(':empty')) {
        var menuHtml = $("#menu").clone();
        var mobileMenu = $("#mySideNav");

        var closeIcon = "<div class='mobileCross' id='mobileCross'><span style='cursor: pointer;' onclick='openNav()'>&#935;</span></div>";

        mobileMenu.append(closeIcon);
        mobileMenu.append(menuHtml);
    }

    if (document.getElementById("mySideNav").style.width === "0px") {

        document.getElementById("mySideNav").style.width = "40vw";
        $("#mobileCross").css("visibility", "visible");
        $("#mySideNav > #menu").css("visibility", "visible");

    } else {

        if (document.getElementById("mySideNav").style.width === "40vw") {

            document.getElementById("mySideNav").style.width = "0";
            $("#mobileCross").css("visibility", "hidden");
            $("#mySideNav > #menu").css("visibility", "hidden");

        } else {

            document.getElementById("mySideNav").style.width = "40vw";
            $("#mobileCross").css("visibility", "visible");
            $("#mySideNav > #menu").css("visibility", "visible");

        }
    }
}

function closeNav() {
    document.getElementById("mySideNav").style.width = "0";
    document.getElementById("body").style.marginLeft = "0";
}


