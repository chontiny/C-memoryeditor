const glitch = 1;
const curse = 2;
const buff = 3;
const miscellaneous = 4;

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
            onBuffsLoaded(JSON.parse(client.getResponseHeader("Buffs")), JSON.parse(client.getResponseHeader("BuffMeta")));
        }
    };
}

function onBuffsLoaded(buffs, buffMeta) {
    if (!buffs) {
        return;
    }

    $('#glitches').empty();
    $('#buffs').empty();
    $('#curses').empty();
    $('#miscellaneous').empty();

    var glitchCount = 0;
    var buffCount = 0;
    var curseCount = 0;
    var miscellaneousCount = 0;
    var i = 0;

    for (i = 0, len = buffs.length; i < len; i++) {
        switch (buffs[i].Category) {
            case glitch:
                glitchCount++;
                $('#glitches').prepend($('<img>', { src: 'Images/Buffs/' + buffs[i].StreamIconPath, class: 'buff img-fluid img-rounded pull-right' }));
                break;
            case buff:
                buffCount++;
                $('#buffs').prepend($('<img>', { src: 'Images/Buffs/' + buffs[i].StreamIconPath, class: 'buff img-fluid img-rounded pull-right' }));
                break;
            case curse:
                curseCount++;
                $('#curses').prepend($('<img>', { src: 'Images/Buffs/' + buffs[i].StreamIconPath, class: 'buff img-fluid img-rounded pull-right' }));
                break;
            case miscellaneous:
                miscellaneousCount++;
                $('#miscellaneous').prepend($('<img>', { src: 'Images/Buffs/' + buffs[i].StreamIconPath, class: 'buff img-fluid img-rounded pull-right' }));
                break;
        }
    }

    for (i = glitchCount, len = buffMeta['NumberOfGlitches']; i < len; i++) {
        $('#glitches').append($('<img>', { src: 'Images/EmptyBuff.svg', class: 'buff img-fluid img-rounded pull-right' }));
    }

    for (i = buffCount, len = buffMeta['NumberOfBuffs']; i < len; i++) {
        $('#buffs').append($('<img>', { src: 'Images/EmptyBuff.svg', class: 'buff img-fluid img-rounded pull-right' }));
    }

    for (i = curseCount, len = buffMeta['NumberOfCurses']; i < len; i++) {
        $('#curses').append($('<img>', { src: 'Images/EmptyBuff.svg', class: 'buff img-fluid img-rounded pull-right' }));
    }

    for (i = miscellaneousCount, len = buffMeta['NumberOfMiscellaneous']; i < len; i++) {
        $('#miscellaneous').append($('<img>', { src: 'Images/EmptyBuff.svg', class: 'buff img-fluid img-rounded pull-right' }));
    }
}