"use strict";

// Shafi: Create connection
var connection = new signalR.HubConnectionBuilder().withUrl("/notificationUserDashboardHub").build();
// End:

// Shafi: When connection is on executive this
connection.on("NotifyLikeUnlike", function (likeDislikeId, message, icon, name, imageId, notificationTime, unreadNotificationCount) {
    $("#notificationPulse").show();
    // Shafi: Append notifications to $("#appendNotifications") in top menu notification of user dashboard

    $("#appendNotifications").prepend("<div class='d-flex justify-content-between border-top'><div class='p-2'><a href=''><div class='main-avatar avatar online'><img alt='avatar' class='rounded-circle avatar' src='/FileManager/ProfileImages/" + imageId + ".jpg'></div></a></div> <div class='p-2'> <strong><a href=''>" + name + "</a></strong> <p>" + message + " <br /><span>" + notificationTime + "</span></p></div> <div class='p-2'> <div class='d-flex flex-column'><div class='p-1'><a href='#0' class='markAsRead'><img src='/icons-notification-green/switch.png' width='20' class='float-right' /></a></div><div class='p-1 mt-2'><img src='" + icon + "' width='20' class='float-right' /></div> </div> </div> </div>");

    $("#unreadNotificationCount").text(unreadNotificationCount);
    // End:
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

    connection.invoke("OnDisconnectedAsync").catch(function (err) {
        console.log(error.toString());
    });
});