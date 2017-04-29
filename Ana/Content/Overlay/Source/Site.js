$(document).ready(function () {
    var client = new XMLHttpRequest();

    client.open("GET", window.location, true);
    client.send();
    client.onreadystatechange = function () {
        if (this.readyState == this.HEADERS_RECEIVED) {
            onBuffsLoaded(client.getResponseHeader("Buffs"));
        }
    }
});

function onBuffsLoaded(buffs) {
}