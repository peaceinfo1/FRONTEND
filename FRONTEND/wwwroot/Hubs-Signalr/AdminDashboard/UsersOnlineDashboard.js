"use strict";

// Shafi: Create connection
var connection = new signalR.HubConnectionBuilder().withUrl("/usersOnlineDashboardHub").build();
// End:

// Shafi: When connection is on executive this
connection.on("UsersOnline", function (count) {

    $("#dashboard").html(count + " Users");

    var subject = "New user has came online. Total users " + count;

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