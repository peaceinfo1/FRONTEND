// Shafi: Signalr

// Shafi: Create connection
var connection = new signalR.HubConnectionBuilder().withUrl("/allConnectedUsersMapHub").build();
// End:

// Shafi: When user connected is on executive this
connection.on("GetAllOnlineUsers", function (allUsers) {
    // Shafi: Begin adding map from http://bl.ocks.org/d3noob/9150014
    var map = L.map('map').setView([19.129102, 72.825430], 2);
    mapLink =
        '<a href="http://openstreetmap.org">OpenStreetMap</a>';
    L.tileLayer(
        'http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; ' + mapLink + ' Contributors',
        maxZoom: 18,
    }).addTo(map);

    // Shafi: Add markers
    for (var i = 0; i < allUsers.length; i++) {
        var shafi = allUsers[i].split(':');

        var con = shafi[0];
        var lat = shafi[1];
        var lon = shafi[2];

        var message = "Connection ID: " + con + " Lattitude: " + lat + " Longitude: " + lon;

        console.log(message);


        marker = L.marker([shafi[1], shafi[2]])
            .addTo(map)
            .bindPopup(shafi[0])
            .openPopup();
    }
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
});
    // End: