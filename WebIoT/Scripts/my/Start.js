

var myMap;
var SetCurrentPos=0;
var DevLocation;
// Дождёмся загрузки API и готовности DOM.
ymaps.ready(init);

function init() {
    // Создание экземпляра карты и его привязка к контейнеру с
    // заданным id ("map").
    myMap = new ymaps.Map('map', {
        // При инициализации карты обязательно нужно указать
        // её центр и коэффициент масштабирования.
        center: [55.76, 37.64], // Москва
        zoom: 10
    }, {
            searchControlProvider: 'yandex#search'
        });
    DevLocation = new ymaps.Placemark([0, 0]);
    myMap.geoObjects.add(DevLocation);
}


$(function () {
    InitEvents();
    //$('#R1ON').show(200);
    //$('#R1OFF').show(300);
    //$('#R2ON').show(400);
    //$('#R2OFF').show(500);
    
});

function InitEvents() {        
    $.getJSON('/Home/RelayGet', RelaySetSuccess);
    loadData();   
    
    $('#R1ON').click({ relay: 1, set: 1 }, RelaySet); // на кнопку прикрутим обработчик
    $('#R1OFF').click({ relay: 1, set: 0 }, RelaySet); // на кнопку прикрутим обработчик
    $('#R2ON').click({ relay: 2, set: 1 }, RelaySet); // на кнопку прикрутим обработчик
    $('#R2OFF').click({ relay: 2, set: 0 }, RelaySet); // на кнопку прикрутим обработчик
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