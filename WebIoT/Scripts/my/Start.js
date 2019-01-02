

var myMap;
var SetCurrentPos=0;
var DevLocation;
ymaps.ready(init);

function init() {    
    myMap = new ymaps.Map('map', {        
        center: [55.76, 37.64],
        zoom: 10
    }, {
            searchControlProvider: 'yandex#search'
        });
    DevLocation = new ymaps.Placemark([0, 0]);
    myMap.geoObjects.add(DevLocation);
    loadData();
}


$(function () {
    InitEvents();
});

function InitEvents() {        
    $.getJSON('/Home/RelayGet', RelaySetSuccess);          
    $('#R1ON').click({ relay: 1, set: 1 }, RelaySet);
    $('#R1OFF').click({ relay: 1, set: 0 }, RelaySet);
    $('#R2ON').click({ relay: 2, set: 1 }, RelaySet);
    $('#R2OFF').click({ relay: 2, set: 0 }, RelaySet);
    $('#FindDev').click(FindDev);
    setInterval('loadData()', 5000);
}
function FindDev() {
    SetCurrentPos = 0;
    loadData();
}

function RelaySet(eventObj) {
    $.getJSON('/Home/RelaySet', { relay: eventObj.data.relay, set: eventObj.data.set }, RelaySetSuccess);
}
function RelaySetSuccess(data) {
    if (data.Rel1 == 0) {
        $('#R1ON').show();
        $('#R1OFF').hide();
    }
    else {
        $('#R1ON').hide();
        $('#R1OFF').show();
    }
    if (data.Rel2 == 0) {
        $('#R2ON').show();
        $('#R2OFF').hide();
    }
    else {
        $('#R2ON').hide();
        $('#R2OFF').show();
    }
}
function loadData() {
    $.getJSON('/Home/LastDataGet', LastDataGetSuccess);
}
function LastDataGetSuccess(data) {
    var text = "<p>Напряжение батареи: " + data.Ubat / 10 + " Вольт</p>";
    text += "<p>Latitude:" + data.Latitude + "</p>";
    text += "<p>Longitude:" + data.Longitude + "</p>";
    text += "<p>Altitude:" + data.MSL_Altitude + "</p>";
    text += "<p>Скорость:" + data.Speed_Over_Ground + "</p>";
    text += "<p>Курс:" + data.Course_Over_Ground + "</p>";
    text += "<p>Точность:" + ((data.HDOP + data.PDOP + data.VDOP)).toFixed(1)  + "</p>";
    $('#BatLev').html(text);
    if (SetCurrentPos == 0) {
        myMap.setCenter([data.Latitude, data.Longitude]);
        myMap.setZoom(18);
        SetCurrentPos = data.Latitude + data.Longitude;
    }    
    DevLocation.geometry.setCoordinates([data.Latitude, data.Longitude]);    
}