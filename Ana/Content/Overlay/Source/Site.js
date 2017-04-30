$(document).ready(function () {
    var client = new XMLHttpRequest();

    client.open("GET", window.location, true);
    client.send();
    client.onreadystatechange = function () {
        if (this.readyState === this.HEADERS_RECEIVED) {
            onBuffsLoaded(JSON.parse(client.getResponseHeader("Buffs")));
        }
    };
});

const glitch = 1;
const curse = 2;
const buff = 3;
const utility = 4;

function onBuffsLoaded(buffs) {
    if (!buffs) {
        return;
    }

    for (var i = 0, len = buffs.length; i < len; i++) {
        switch (buffs[i].Category) {
            case glitch:
                $('#glitches').prepend($('<img>', { src: 'Images/Buffs/' + buffs[i].StreamIconPath, class: 'buff img-fluid img-rounded pull-right' }));
                break;
            case buff:
                $('#buffs').prepend($('<img>', { src: 'Images/Buffs/' + buffs[i].StreamIconPath, class: 'buff img-fluid img-rounded pull-right' }));
                break;
            case curse:
                $('#curses').prepend($('<img>', { src: 'Images/Buffs/' + buffs[i].StreamIconPath, class: 'buff img-fluid img-rounded pull-right' }));
                break;
            case utility:
                $('#utilities').prepend($('<img>', { src: 'Images/Buffs/' + buffs[i].StreamIconPath, class: 'buff img-fluid img-rounded pull-right' }));
                break;
        }
    }
}