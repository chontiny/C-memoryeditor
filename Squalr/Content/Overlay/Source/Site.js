const glitch = 1;
const curse = 2;
const buff = 3;
const utility = 4;

$(document).ready(function () {
    $(function () {
        startRefresh();
    });
});

function startRefresh() {
    setTimeout(startRefresh, 1000);

    var client = new XMLHttpRequest();

    client.open("GET", window.location, true);
    client.send();
    client.onreadystatechange = function () {
        if (this.readyState === this.HEADERS_RECEIVED) {
            onBuffsLoaded(JSON.parse(client.getResponseHeader("Buffs")));
        }
    };
}

function onBuffsLoaded(buffs) {
    if (!buffs) {
        return;
    }

    $('#glitches').empty();
    $('#buffs').empty();
    $('#curses').empty();
    $('#utilities').empty();

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