// Shafi: Signalr
// Shafi: Get full current url of client
var url = document.URL;
var referrer = document.referrer;
// End:

// Shafi: Create connection
var connection = new signalR.HubConnectionBuilder().withUrl("/allConnectedUsersHub?url=" + url + "&referrer=" + referrer + "").build();
// End:

// Shafi: When user connected is on executive this
connection.on("UserConnected", function (userDetails) {

    $("#allUsers").append("<tr id='" + userDetails.connectionId + "'><td><div class='avatar brround d-block cover-image' data-image-src='../../assets/img/faces/5.jpg' style='background: url(&quot;" + userDetails.profileImage + "&quot;) center center;'><span class='avatar-status bg-green'></span></div></td><td>" + userDetails.userType + "</td><td>" + userDetails.userName + "</td><td>" + userDetails.country + "</td><td>" + userDetails.state + "</td><td>" + userDetails.city + "</td><td>" + userDetails.postal + "</td><td>" + userDetails.latitude + ", " + userDetails.longitude + "</td><td><a href='" + userDetails.url + "' target='_blank''>" + userDetails.url + "</a></td><td><a href='" + userDetails.referrer + "' target='_blank''>" + userDetails.referrer + "</a></td></tr>");

    var subject = 'New user came';

    $.notify(
        subject,
        {
            autoHide: true,
            autoHideDelay: 5000,
            className: 'success'
        }
    );
});
// End:

// Shafi: When user disconnected is on executive this
connection.on("UserDisconnected", function (connectionId) {

    var removeUser = "#" + connectionId;

    $(removeUser).remove();

    var subject = 'A user gone away';

    $.notify(
        subject,
        {
            autoHide: true,
            autoHideDelay: 5000,
            className: 'success'
        }
    );
});
// End:

// Shafi: Start connection
connection.start().then(function () {
}).catch(function (err) {
    console.log(err.toString());
});
// End:

// Shafi: Invoke connection
$(document).ready(function () {
    connection.invoke("OnConnectedAsync").catch(function (err) {
        console.log(error.toString());
    });
});
    // End: